using QuoteFetcher.MenuSystem.ShowMenu;
using MediatR;
using NSubstitute;

namespace QuoteFetcher.UnitTests.MenuSystem.ShowMenu;

[TestFixture]
public class ShowMenuItemTests
{
    [Test]
    public void Should_Send_ShowMenu_Message()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var menuItem = new ShowMenuItem(sender);
        
        // Act
        menuItem.Invoke(CancellationToken.None);
        
        // Assert
        sender.Received().Send(Arg.Any<ShowMenuRequest>(), CancellationToken.None);
    }
}