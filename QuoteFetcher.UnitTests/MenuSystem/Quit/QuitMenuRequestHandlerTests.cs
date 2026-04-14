using QuoteFetcher.MenuSystem.Quit;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace QuoteFetcher.UnitTests.MenuSystem.Quit;

[TestFixture]
public class QuitMenuRequestHandlerTests
{
    [Test]
    public void Should_StopApplication()
    {
        // Arrange
        var lifetime = Substitute.For<IHostApplicationLifetime>();
        var consoleWrapper = Substitute.For<IConsoleWrapper>();
        var handler = new QuitMenuRequestHandler(lifetime, consoleWrapper);
        
        // Act
        handler.Handle(new QuitMenuRequest(), CancellationToken.None);
        
        // Assert
        lifetime.Received().StopApplication();
    }
}