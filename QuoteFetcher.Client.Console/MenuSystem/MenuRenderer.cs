namespace QuoteFetcher.MenuSystem;

public interface IMenuRenderer
{
    void Display();
}

internal sealed class MenuRenderer(
    IMenuItems menuItems, 
    IConsoleWrapper consoleWrapper) 
    : IMenuRenderer
{
    public void Display()
    {
        consoleWrapper.WriteLine(new string('-', 80));
        foreach (var menuItem in menuItems)
        {
            if (!menuItem.Hidden)
            {
                consoleWrapper.WriteLine(menuItem.Prompt);
            }
        }
    }
}