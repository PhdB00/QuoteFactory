using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using QuoteFetcher.Abstractions.Messaging;
using QuoteFetcher.Application.Abstractions;
using QuoteFetcher.Application.Quotes.GetQuotes;
using QuoteFetcher.Prompts.GetQuotes;

namespace QuoteFetcher.MenuSystem.GetQuotes;

internal sealed class GetQuotesMenuRequestHandler(
    ILogger<GetQuotesMenuRequestHandler> logger,
    ISender sender,
    IConsoleWrapper consoleWrapper,
    IGetQuotesPromptProvider getQuotesPromptProvider) 
    : IMenuRequestHandler<GetQuotesMenuRequest, int>
{
    public async Task<Result<int>> Handle(
        GetQuotesMenuRequest request, 
        CancellationToken cancellationToken)
    {
        getQuotesPromptProvider.ExecutePromptSequence();
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Application cancellation has been requested.");
            // While the Prompt Sequence was executing, application cancellation was requested.
            // This should NOT be treated as a Failure. In order to exit cleanly, return
            // zero as if no quotes were found.
            return Result.Success(0); 
        }
        
        var quoteRequest = new GetQuotesStreamQuery(
            getQuotesPromptProvider.NumberOfQuotes, 
            getQuotesPromptProvider.Category);
        
        logger.LogInformation("User has requested {Num} Quote(s) for category {Cat}", 
            quoteRequest.NumberOfQuotes, quoteRequest.Category);
        
        var received = 0;
        try
        {
            await foreach (var quote in
                           sender.CreateStream(quoteRequest, cancellationToken))
            {
                received++;
                logger.LogInformation("Quote {Rec} of {Num} received, category [{Cat}].",
                    received, getQuotesPromptProvider.NumberOfQuotes, getQuotesPromptProvider.Category);
                consoleWrapper.WriteLine(quote);
            }
        }
        catch (ValidationException e)
        {
            logger.LogError(e, "Validation error occurred.");
            return Result.Failure<int>(e);
        }
        
        WriteSummary(received, 
            getQuotesPromptProvider.NumberOfQuotes, 
            getQuotesPromptProvider.Category);
        
        return received;
    }

    private void WriteSummary(int received, int requested, string category)
    {
        consoleWrapper.WriteLine();
        if (received < requested)
        {
            logger.LogDebug("Only {Rec} of {Num} quotes were received", received, requested);
            consoleWrapper.WriteLine($"Sorry, we could only find {received} {(!string.IsNullOrEmpty(getQuotesPromptProvider.Category) ? category : string.Empty)} quotes. Better luck next time!");
        }
        else
        {
            logger.LogDebug("{Rec} quotes were received", received);
            consoleWrapper.WriteLine("We hope you enjoyed those quotes :-) Please feel free to ask for more.");
        }
        consoleWrapper.WriteLine();
    }
}