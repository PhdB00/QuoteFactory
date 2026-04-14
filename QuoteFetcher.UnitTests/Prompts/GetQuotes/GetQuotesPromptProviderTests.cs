using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QuoteFetcher.Application.Categories.GetCategories;
using QuoteFetcher.Prompts.GetQuotes;

namespace QuoteFetcher.UnitTests.Prompts.GetQuotes;

[TestFixture]
public class GetQuotesPromptProviderTests
{
    private ISender sender;
    private IConsoleWrapper consoleWrapper;
    private List<IGetQuotesPrompt> getQuotesPrompts;
    private IGetQuotesPrompt numberPrompt;
    private IGetQuotesPrompt useCategoryPrompt;
    private IGetQuotesPrompt enterCategoryPrompt;
    
    [SetUp]
    public void Setup()
    {
        sender = Substitute.For<ISender>();
        numberPrompt = new NumberOfQuotesPrompt();
        useCategoryPrompt = new UseCategoryPrompt();
        enterCategoryPrompt = new EnterCategoryPrompt(
            sender, Substitute.For<ILogger<EnterCategoryPrompt>>());
        
        consoleWrapper = Substitute.For<IConsoleWrapper>();
        getQuotesPrompts =
        [
            numberPrompt,
            useCategoryPrompt,
            enterCategoryPrompt
        ];
    }
    
    [Test]
    public void Should_Execute_Prompt_Sequence_Without_Category()
    {
        // Arrange
        consoleWrapper
            .ReadKey()
            .Returns(
                new ConsoleKeyInfo('5', ConsoleKey.D5, false, false, false),
                new ConsoleKeyInfo('N', ConsoleKey.N, false, false, false));

        var provider = new GetQuotesPromptProvider(
            consoleWrapper, 
            getQuotesPrompts);
        
        // Act
        var result = provider.ExecutePromptSequence();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(provider.Category, Is.Empty);
            Assert.That(provider.NumberOfQuotes, Is.EqualTo(5));
        });
    }
    
    [Test]
    public void Should_Execute_Prompt_Sequence_With_Category()
    {
        // Arrange
        sender.Send(Arg.Any<GetCategoriesQuery>())
            .Returns(new List<string>{ "hund", "kat" });
        
        consoleWrapper
            .ReadKey()
            .Returns(
                new ConsoleKeyInfo('3', ConsoleKey.D3, false, false, false),
                new ConsoleKeyInfo('y', ConsoleKey.Y, false, false, false));

        consoleWrapper
            .ReadLine()
            .Returns("kat");
        
        var provider = new GetQuotesPromptProvider(
            consoleWrapper, 
            getQuotesPrompts);
        
        // Act
        var result = provider.ExecutePromptSequence();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(provider.Category, Is.EqualTo("kat"));
            Assert.That(provider.NumberOfQuotes, Is.EqualTo(3));
        });
    }
}