using System.Threading;
using System.Threading.Tasks;
using QuoteFetcher.Application.Abstractions.Messaging;

namespace QuoteFetcher.MenuSystem.GetUserSelection;

internal sealed class GetUserSelectionRequestHandler(
    IMenuProvider menuProvider)
    : IQueryHandler<GetUserSelectionRequest>
{
    public Task Handle(GetUserSelectionRequest request, 
        CancellationToken cancellationToken)
    {
        menuProvider.ProcessUserSelection();
        return Task.CompletedTask;
    }
}