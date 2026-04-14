using System.Runtime.CompilerServices;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuoteFetcher.Application.Abstractions.Api;
using QuoteFetcher.Application.Abstractions.Messaging;

namespace QuoteFetcher.Application.Quotes.GetQuotes;

internal sealed class GetQuotesStreamQueryHandler
(
    IOptions<QuoteApiSettings> settings,
    ILogger<GetQuotesStreamQueryHandler> logger,
    IQuoteApi api,
    IValidator<GetQuotesStreamQuery> validator,
    IQuoteHashSet quoteHashSet)
    : IStreamQueryHandler<GetQuotesStreamQuery, string>
{
    // Semaphore to limit the number of concurrent API calls.
    private static readonly SemaphoreSlim Semaphore = new(0);

    public async IAsyncEnumerable<string> Handle(GetQuotesStreamQuery getQuotesStreamQuery, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var category = getQuotesStreamQuery.Category.ToLower();
        logger.LogDebug("Handling GetQuotesRequest: NumberOfQuote:[{Num}] Category:[{Cat}]", getQuotesStreamQuery.NumberOfQuotes, category);
        
        var validationResult = await validator.ValidateAsync(getQuotesStreamQuery, cancellationToken);
        if (!validationResult.IsValid)
        {
            var validationErrors = string.Join("|", validationResult.Errors.SelectMany(x => x.ErrorMessage));
            logger.LogInformation("Validation failed: {Errors}", validationErrors);
            throw new ValidationException(validationErrors);
        }
        
        var quoteQueryParameters = new GetQuoteQueryParameters
        {
            Category = category
        };

        // The API allows us to request and receive a single Quote, but it may return a Quote that
        // we have already seen. We must ensure that a single user request receives a distinct
        // set of quotes which means that a request for n quotes may require MORE THAN n API
        // calls. 
        //
        // We cannot keep calling the API infinitely until we receive a complete set of unique
        // quotes because we do not know how many quotes are in the underlying database, so have
        // no way of determining whether the request can ever be fully satisfied and cannot keep
        // the user waiting forever.
        //
        // The AppSettings value MaxRetryOnDuplicate provides a value for a number of
        // additional API calls to make in the event that the user request is not satisfied by sending
        // a number equal to the number of quotes requested.
        //
        // We will make an absolute maximum of (n + MaxRetryOnDuplicate) API calls to make our
        // best effort to satisfy the user request. 
        
        var grandTotalNumberOfRequestsAllowed = MaximumNumberOfQuoteRequests(getQuotesStreamQuery.NumberOfQuotes);
        
        using var cts = new CancellationTokenSource();
        
        var tasks = new List<Task<Quote>>(grandTotalNumberOfRequestsAllowed);
        for (var i = 0; i < grandTotalNumberOfRequestsAllowed; i++)
        {
            // Queue the API calls with Task.Run then process the results using Task.WhenEach.
            // We will yield the individual quotes as we receive them so that the presentation
            // layer is not waiting for the total number of quotes requested before displaying
            // anything. This will make a more satisfying user experience.
            // Note we use a Semaphore to control the rate at which we make the API calls. 
            tasks.Add(
                Task.Run(async () =>
                    {
                        await Semaphore.WaitAsync(cts.Token);
                        var result = await api.GetQuoteAsync(quoteQueryParameters);
                        Semaphore.Release();
                        return result;
                    },
                    cts.Token));
        }
        
        logger.LogDebug("Waiting for a maximum of {Max} GetQuoteAsync Tasks to complete", grandTotalNumberOfRequestsAllowed);
        Semaphore.Release(settings.Value.MaxConcurrentApiCalls);
        
        await foreach (var task in Task
                           .WhenEach(tasks)
                           .WithCancellation(cts.Token)
                      )
        {
            var quote = await task;
            
            // We have received a Quote from the Api: before processing, check whether the
            // number of quotes requested by the user has already been received.
            if (quoteHashSet.Count >= getQuotesStreamQuery.NumberOfQuotes)
            {
                break;
            }
        
            var (quoteIsUnique, numberInHashSet) = quoteHashSet.AddUniqueAndCount(quote);
            if (quoteIsUnique && numberInHashSet <= getQuotesStreamQuery.NumberOfQuotes)
            {
                // We have received a unique Quote
                yield return quote.Text;
            }
        
            // If we have received the number of quotes requested by the user, break
            // out of the loop.
            if (quoteHashSet.Count >= getQuotesStreamQuery.NumberOfQuotes)
            {
                break;
            }
        }
        
        await cts.CancelAsync();
        
        logger.LogDebug("GetQuotesStreamQueryHandler has completed with {Pending} Quote(s) pending of {Requested} requested by {Total} tasks",
            quoteHashSet.Count, 
            getQuotesStreamQuery.NumberOfQuotes, 
            grandTotalNumberOfRequestsAllowed);
    }
    
    private int MaximumNumberOfQuoteRequests(int numberOfQuotesRequestedByUser)
    {
        if (numberOfQuotesRequestedByUser == 1)
        {
            return 1; // No issues with duplicates to consider.
        }
        return numberOfQuotesRequestedByUser + settings.Value.MaxRetryOnDuplicate;
    }
}