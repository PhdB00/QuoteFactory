using QuoteFetcher.MenuSystem.Invalid;
using MediatR;
using NSubstitute;

namespace QuoteFetcher.UnitTests.MenuSystem.Invalid;

[TestFixture]
public class InvalidMenuItemTests
{
    [Test]
    public void Should_Send_InvalidMenuRequest()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var menuItem = new InvalidMenuItem(sender);
        
        // Act
        _ = menuItem.Invoke(CancellationToken.None);
        
        // Assert
        sender.Received().Send(Arg.Any<InvalidMenuRequest>(), CancellationToken.None);
    }
}