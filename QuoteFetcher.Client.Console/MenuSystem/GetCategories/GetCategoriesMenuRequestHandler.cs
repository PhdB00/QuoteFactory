using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuoteFetcher.Abstractions.Messaging;
using QuoteFetcher.Application.Abstractions;
using QuoteFetcher.Application.Categories.GetCategories;
using MediatR;

namespace QuoteFetcher.MenuSystem.GetCategories;

internal sealed class GetCategoriesMenuRequestHandler(ISender sender,
    IConsoleWrapper consoleWrapper) 
    : IMenuRequestHandler<GetCategoriesMenuRequest, int>
{
    public async Task<Result<int>> Handle(GetCategoriesMenuRequest menuRequest, 
        CancellationToken cancellationToken)
    {
        var categoryResult = sender.Send(new GetCategoriesQuery(), cancellationToken);
        categoryResult.Wait(1000, cancellationToken);
        if (!categoryResult.IsCompleted)
        {
            consoleWrapper.WriteLine("Waiting for categories to be available...");
            await categoryResult;
        }
        
        if (cancellationToken.IsCancellationRequested)
        {
            // While we were waiting for the Categories to be returned from the API, application cancellation was requested.
            // This should NOT be treated as a Failure. In order to exit cleanly, return
            // zero as if no categories were found.
            return Result.Success(0);
        }
        
        var categories = categoryResult.Result.Value;
        consoleWrapper.WriteLine(categories.Any()
            ? $"Categories are: {string.Join(",", categories)}"
            : "No categories found.");
        return categories.Count;
    }
}