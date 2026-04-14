using MediatR;
using NSubstitute;
using QuoteFetcher.MenuSystem.GetQuotes;

namespace QuoteFetcher.UnitTests.MenuSystem.GetQuotes;

[TestFixture]
public class GetQuotesMenuItemTests
{
    [Test]
    public void Should_Send_Request()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var menuitem = new GetQuotesMenuItem(sender);
        
        // Act
        menuitem.Invoke(CancellationToken.None);
        
        // Assert 
        sender.Received().Send(Arg.Any<GetQuotesMenuRequest>(), CancellationToken.None);
    }
}