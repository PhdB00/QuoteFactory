using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuoteFetcher.MenuSystem;

public interface IMenuProvider
{
    void ProcessUserSelection();
}

internal sealed class MenuProvider(
    IHostApplicationLifetime lifetime,
    ILogger<MenuProvider> logger,
    IConsoleWrapper consoleWrapper,
    IMenuItems menuItems)
    : IMenuProvider
{
    public void ProcessUserSelection()
    {
        var keys = string.Join(", ", menuItems.MenuKeys());
        while (!lifetime.ApplicationStopping.IsCancellationRequested)
        {
            consoleWrapper.WriteLine($"Please choose from the menu: (options: {keys})");
            var key = consoleWrapper.ReadKey();
            consoleWrapper.WriteLine();
            var menuItem = menuItems.GetItemByMenuCharacter(key.KeyChar);
            ProcessMenuItem(menuItem);
        }
    }

    private void ProcessMenuItem(IMenuItem menuItem)
    {
        try
        {
            var menuTask = menuItem?.Invoke(lifetime.ApplicationStopping); 
            while (menuTask is { IsCompleted: false })
            {
                menuTask.Wait(100);
            }
        }
        catch (AggregateException ae)
        {
            ae.Handle(e =>
            {
                if (e is OperationCanceledException)
                {
                    logger.LogInformation("The menu operation has been Canceled");
                    return true;
                }
                logger.LogError(ae, "Exception occurred processing Menu option [{Menu}]", menuItem?.Prompt);       
                return true;
            });
        }
        
    }
}