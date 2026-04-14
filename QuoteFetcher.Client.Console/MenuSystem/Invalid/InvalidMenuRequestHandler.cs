using System.Threading;
using System.Threading.Tasks;
using QuoteFetcher.Abstractions.Messaging;

namespace QuoteFetcher.MenuSystem.Invalid;

internal sealed class InvalidMenuRequestHandler(IConsoleWrapper consoleWrapper) 
    : IMenuRequestHandler<InvalidMenuRequest>
{
    public Task Handle(InvalidMenuRequest request, CancellationToken cancellationToken)
    {
        consoleWrapper.WriteLine(string.Empty);
        return Task.CompletedTask;
    }
}