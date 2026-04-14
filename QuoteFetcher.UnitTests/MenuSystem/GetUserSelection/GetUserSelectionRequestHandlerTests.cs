using QuoteFetcher.MenuSystem;
using QuoteFetcher.MenuSystem.GetUserSelection;
using NSubstitute;

namespace QuoteFetcher.UnitTests.MenuSystem.GetUserSelection;

[TestFixture]
public class GetUserSelectionRequestHandlerTests
{
    [Test]
    public void Should_Call_GetUserSelectionRequestHandler()
    {
        // Arrange
        var menuProvider = Substitute.For<IMenuProvider>();
        var handler = new GetUserSelectionRequestHandler(menuProvider);
        
        // Act
        var task = handler.Handle(new GetUserSelectionRequest(), CancellationToken.None);
        
        // Assert
        menuProvider.Received().ProcessUserSelection();
        Assert.That(task.IsCompletedSuccessfully, Is.True);
    }
}