using System;
using System.Threading;
using System.Threading.Tasks;
using QuoteFetcher.MenuSystem.GetUserSelection;
using QuoteFetcher.MenuSystem.ShowMenu;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuoteFetcher;

public class ConsoleHostedService(
    ILogger<ConsoleHostedService> logger,
    IHostApplicationLifetime lifetime,
    ISender sender)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Application started.");
            await LaunchUserMenuAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unhandled exception!");
        }
        finally
        {
            logger.LogInformation("Stopping application.");
            lifetime.StopApplication();
        }
    }

    private async Task LaunchUserMenuAsync(CancellationToken cancellationToken)
    {
        await sender.Send(new ShowMenuRequest(), cancellationToken);
        await sender.Send(new GetUserSelectionRequest(), cancellationToken);
    }
}