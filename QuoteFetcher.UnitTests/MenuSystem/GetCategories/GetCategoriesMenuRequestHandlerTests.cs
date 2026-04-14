using QuoteFetcher.Application.Categories.GetCategories;
using QuoteFetcher.MenuSystem.GetCategories;
using MediatR;
using NSubstitute;

namespace QuoteFetcher.UnitTests.MenuSystem.GetCategories;

[TestFixture]
public class GetCategoriesMenuRequestHandlerTests
{
    [Test]
    public void Should_Return_Categories()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var consoleWrapper = Substitute.For<IConsoleWrapper>();

        var categoryList = new List<string>
        {
            "test1",
            "test2"
        };
        sender.Send(
                Arg.Any<GetCategoriesQuery>()
                , CancellationToken.None)
            .Returns(categoryList);
        
        var handler = new GetCategoriesMenuRequestHandler(sender, consoleWrapper);

        // Act
        var task = handler.Handle(new GetCategoriesMenuRequest(), CancellationToken.None);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(task.Result.IsSuccess, Is.True);
            Assert.That(task.Result.Value, Is.EqualTo(2));
        });
        sender.Received().Send(Arg.Any<GetCategoriesQuery>(), CancellationToken.None);
    }
}