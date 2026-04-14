using QuoteFetcher.MenuSystem.Quit;
using MediatR;
using NSubstitute;

namespace QuoteFetcher.UnitTests.MenuSystem.Quit;

[TestFixture]
public class QuitMenuItemTests
{
    [Test]
    public void Should_Send_Quit_Message()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var menuItem = new QuitMenuItem(sender);
        
        // Act
        menuItem.Invoke(CancellationToken.None);

        // Assert
        sender.Received().Send(Arg.Any<QuitMenuRequest>(), CancellationToken.None);
    }
}