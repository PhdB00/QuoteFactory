using QuoteFetcher.MenuSystem;
using QuoteFetcher.MenuSystem.ShowMenu;
using NSubstitute;

namespace QuoteFetcher.UnitTests.MenuSystem.ShowMenu;

[TestFixture]
public class ShowMenuRequestHandlerTests
{
    [Test]
    public void ShowMenuRequestHandler_HandlesNullInput()
    {
        // Arrange
        var menuRenderer = Substitute.For<IMenuRenderer>();
        var handler = new ShowMenuRequestHandler(menuRenderer);

        // Act
        handler.Handle(new ShowMenuRequest(), CancellationToken.None);
        
        // Assert
        menuRenderer.Received(1).Display();
    }
}