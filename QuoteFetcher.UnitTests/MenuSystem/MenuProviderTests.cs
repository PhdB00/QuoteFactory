using QuoteFetcher.MenuSystem;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace QuoteFetcher.UnitTests.MenuSystem;

[TestFixture]
public class MenuProviderTests
{
    private IHostApplicationLifetime lifetime;
    private ILogger<MenuProvider> logger;
    private IConsoleWrapper consoleWrapper;
    private IMenuItems menuItems;

    [SetUp]
    public void Setup()
    {
        lifetime = Substitute.For<IHostApplicationLifetime>();
        logger = Substitute.For<ILogger<MenuProvider>>();
        consoleWrapper = Substitute.For<IConsoleWrapper>();
        menuItems = Substitute.For<IMenuItems>();
    }
    
    [Test]
    public void Should_Process_MenuItem_Until_Cancelled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        
        var token = cts.Token;

        var keyA = new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false);
        consoleWrapper.ReadKey().Returns(keyA);

        menuItems.MenuKeys().Returns(["?", "a", "b", "c"]);
        
        var menuItem = Substitute.For<IMenuItem>();
        menuItems.GetItemByMenuCharacter(Arg.Any<char>())
            .Returns(menuItem);
        
        menuItems
            .When(x => x.GetItemByMenuCharacter('a'))
            .Do(_ => CallbackCancel());
            
        var menuProvider = new MenuProvider(
            lifetime,
            logger,
            consoleWrapper,
            menuItems);
        
        // Act
        menuProvider.ProcessUserSelection();

        // Assert
        menuItem.Received().Invoke(Arg.Any<CancellationToken>());

        return;
        
        void CallbackCancel()
        {
            cts.Cancel();
            lifetime.ApplicationStopping.Returns(token);
        }
    }
}