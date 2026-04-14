using QuoteFetcher.MenuSystem.GetCategories;
using MediatR;
using NSubstitute;

namespace QuoteFetcher.UnitTests.MenuSystem.GetCategories;

[TestFixture]
public class GetCategoriesMenuItemTests
{
    [Test]
    public void Should_Send_Request()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var menuitem = new GetCategoriesMenuItem(sender);
        
        // Act
        menuitem.Invoke(CancellationToken.None);
        
        // Assert 
        sender.Received().Send(Arg.Any<GetCategoriesMenuRequest>(), CancellationToken.None);
    }
}