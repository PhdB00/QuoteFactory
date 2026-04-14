using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QuoteFetcher.Application.Quotes.GetQuotes;
using QuoteFetcher.MenuSystem.GetQuotes;
using QuoteFetcher.Prompts.GetQuotes;

namespace QuoteFetcher.UnitTests.MenuSystem.GetQuotes;

[TestFixture]
public class GetQuotesMenuRequestHandlerTests
{
    private ILogger<GetQuotesMenuRequestHandler> logger;
    private ISender sender;
    private IConsoleWrapper consoleWrapper;
    private IGetQuotesPromptProvider getQuotesPromptProvider;

    [SetUp]
    public void SetUp()
    {
        logger = Substitute.For<ILogger<GetQuotesMenuRequestHandler>>();
        sender = Substitute.For<ISender>();
        consoleWrapper = Substitute.For<IConsoleWrapper>();
        getQuotesPromptProvider = Substitute.For<IGetQuotesPromptProvider>();
    }
    
    [Test]
    public void Should_Return_Fewer_Quotes_Than_Requested()
    {
        // Arrange
        getQuotesPromptProvider.NumberOfQuotes.Returns(2);
        getQuotesPromptProvider.Category.Returns("test");
        
        var quotesAsyncEnumerable = new List<string>().ToAsyncEnumerable();
        
        sender
            .CreateStream(Arg.Any<GetQuotesStreamQuery>(), 
                CancellationToken.None)
            .Returns(_ => quotesAsyncEnumerable);
        
        var handler = new GetQuotesMenuRequestHandler(
            logger,
            sender, 
            consoleWrapper, 
            getQuotesPromptProvider);
        
        // Act
        var task = handler.Handle(new GetQuotesMenuRequest(), CancellationToken.None);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(task.Result.IsSuccess, Is.True);
            Assert.That(task.Result.Value, Is.Zero);
        });
        sender.Received().CreateStream(Arg.Any<GetQuotesStreamQuery>(), Arg.Any<CancellationToken>());
    }
    
    [Test]
    public void Should_Return_Total_Number_of_Quotes_Requested()
    {
        // Arrange
        getQuotesPromptProvider.NumberOfQuotes.Returns(2);
        getQuotesPromptProvider.Category.Returns("test");

        var quoteList = new List<string>
        {
            "quote1",
            "quote2" 
        };
        var quoteAsyncEnumerable = quoteList.ToAsyncEnumerable();
        
        sender
            .CreateStream(Arg.Any<GetQuotesStreamQuery>(), 
                CancellationToken.None)
            .Returns(_ => quoteAsyncEnumerable);
        
        var handler = new GetQuotesMenuRequestHandler(
            logger,
            sender, 
            consoleWrapper, 
            getQuotesPromptProvider);
        
        // Act
        var task = handler.Handle(new GetQuotesMenuRequest(), CancellationToken.None);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(task.Result.IsSuccess, Is.True);
            Assert.That(task.Result.Value, Is.EqualTo(2));
        });
        sender.Received().CreateStream(Arg.Any<GetQuotesStreamQuery>(), 
            Arg.Any<CancellationToken>());
    }
}