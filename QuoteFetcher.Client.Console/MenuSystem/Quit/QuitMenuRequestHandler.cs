using System.Threading;
using System.Threading.Tasks;
using QuoteFetcher.Abstractions.Messaging;
using Microsoft.Extensions.Hosting;

namespace QuoteFetcher.MenuSystem.Quit;

internal sealed class QuitMenuRequestHandler(
    IHostApplicationLifetime lifetime,
    IConsoleWrapper consoleWrapper) 
    : IMenuRequestHandler<QuitMenuRequest>
{
    public Task Handle(QuitMenuRequest request, CancellationToken cancellationToken)
    {
        consoleWrapper.WriteLine("Application is shutting down...");
        lifetime.StopApplication();
        return Task.CompletedTask;
    }
}