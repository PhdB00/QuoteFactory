using QuoteFetcher.MenuSystem.Invalid;
using NSubstitute;

namespace QuoteFetcher.UnitTests.MenuSystem.Invalid;

[TestFixture]
public class InvalidMenuRequestHandlerTests
{
    [Test]
    public void Should_Handle_Invalid_Menu_Request()
    {
        // Arrange
        var consoleWrapper = Substitute.For<IConsoleWrapper>();
        var handler = new InvalidMenuRequestHandler(consoleWrapper);
        
        // Act
        var task = handler.Handle(new InvalidMenuRequest(), CancellationToken.None);
        
        // Assert
        consoleWrapper.Received(1).WriteLine(string.Empty);
        Assert.That(task.IsCompletedSuccessfully, Is.True);
    }
}