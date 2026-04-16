using System.Runtime.CompilerServices;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuoteFetcher.Application.Abstractions.Api;
using QuoteFetcher.Application.Abstractions.Messaging;

namespace QuoteFetcher.Application.Quotes.GetQuotes;

#pragma warning disable S3776 // Disable Cognitive Complexity warning as this is a stream query handler.
internal sealed class GetQuotesStreamQueryHandler
(
    IOptions<QuoteApiSettings> settings,
    ILogger<GetQuotesStreamQueryHandler> logger,
    IQuoteApi api,
    IValidator<GetQuotesStreamQuery> validator,
    IQuoteHashSet quoteHashSet)
    : IStreamQueryHandler<GetQuotesStreamQuery, string>
{
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
        // quotes as we do not know exactly how many quotes exist in the underlying database. 
        // As there is no way of determining whether the request can ever be fully satisfied, we cannot keep
        // the user waiting forever for their complete request.
        //
        // The AppSettings value MaxRetryOnDuplicate provides a value for a number of
        // additional API calls to make in the event that the user request is not satisfied by sending
        // a number equal to the number of quotes requested.
        //
        // We will make an absolute maximum of (n + MaxRetryOnDuplicate) API calls to make our
        // best effort to satisfy the user request. 
        
        var grandTotalNumberOfRequestsAllowed = MaximumNumberOfQuoteRequests(getQuotesStreamQuery.NumberOfQuotes);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var throttler = new SemaphoreSlim(
            settings.Value.MaxConcurrentApiCalls,
            settings.Value.MaxConcurrentApiCalls);

        var token = linkedCts.Token;
        var tasks = GetQuoteTasks(grandTotalNumberOfRequestsAllowed, 
            quoteQueryParameters, throttler, token);
        
        logger.LogDebug("Waiting for a maximum of {Max} GetQuoteAsync Tasks to complete", grandTotalNumberOfRequestsAllowed);
        try
        {
            await foreach (var task in Task
                               .WhenEach(tasks)
                               .WithCancellation(token)
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
        }
        finally
        {
            await linkedCts.CancelAsync();
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // Expected when the stream has enough quotes or caller cancels.
            }
            catch
            {
                // Do not mask the primary exception path from task processing.
            }
        }

        logger.LogDebug("GetQuotesStreamQueryHandler has completed with {Pending} Quote(s) pending of {Requested} requested by {Total} tasks",
            quoteHashSet.Count, 
            getQuotesStreamQuery.NumberOfQuotes, 
            grandTotalNumberOfRequestsAllowed);
    }

    private List<Task<Quote>> GetQuoteTasks(int grandTotalNumberOfRequestsAllowed, 
        GetQuoteQueryParameters quoteQueryParameters,
        SemaphoreSlim throttler,
        CancellationToken token)
    {
        var tasks = new List<Task<Quote>>(grandTotalNumberOfRequestsAllowed);
        for (var i = 0; i < grandTotalNumberOfRequestsAllowed; i++)
        {
            // Queue the API calls with Task.Run, then process the results using Task.WhenEach.
            // We will yield the individual quotes as we receive them so that the presentation
            // layer is not waiting for the total number of quotes requested before displaying
            // anything. This will make a more satisfying user experience.
            // Note we use a Semaphore to control the rate at which we make the API calls. 
            tasks.Add(GetQuoteWithThrottleAsync(quoteQueryParameters, throttler, token));
        }

        return tasks;
    }
    
    private int MaximumNumberOfQuoteRequests(int numberOfQuotesRequestedByUser)
    {
        if (numberOfQuotesRequestedByUser == 1)
        {
            return 1; // No issues with duplicates to consider.
        }
        return numberOfQuotesRequestedByUser + settings.Value.MaxRetryOnDuplicate;
    }

    private async Task<Quote> GetQuoteWithThrottleAsync(
        GetQuoteQueryParameters quoteQueryParameters,
        SemaphoreSlim throttler,
        CancellationToken cancellationToken)
    {
        await throttler.WaitAsync(cancellationToken);
        try
        {
            return await api.GetQuoteAsync(quoteQueryParameters);
        }
        finally
        {
            throttler.Release();
        }
    }
}
#pragma warning restore S3776