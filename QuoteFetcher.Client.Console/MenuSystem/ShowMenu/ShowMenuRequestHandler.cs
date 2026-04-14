using System.Threading;
using System.Threading.Tasks;
using QuoteFetcher.Application.Abstractions.Messaging;

namespace QuoteFetcher.MenuSystem.ShowMenu;

internal sealed class ShowMenuRequestHandler(IMenuRenderer menuRenderer) 
    : IQueryHandler<ShowMenuRequest>
{
    public Task Handle(ShowMenuRequest request, 
        CancellationToken cancellationToken)
    {
        menuRenderer.Display();
        return Task.CompletedTask;
    }
}