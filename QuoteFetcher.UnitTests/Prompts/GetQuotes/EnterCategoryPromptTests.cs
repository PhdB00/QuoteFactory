using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QuoteFetcher.Application.Categories.GetCategories;
using QuoteFetcher.Prompts.GetQuotes;

namespace QuoteFetcher.UnitTests.Prompts.GetQuotes;

[TestFixture]
public class EnterCategoryPromptTests
{
    [Test]
    public void Should_Load_Categories()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var logger = Substitute.For<ILogger<EnterCategoryPrompt>>();
        sender.Send(Arg.Any<GetCategoriesQuery>())
            .Returns(new List<string>{ "abc", "def" });
        
        // Act
        var prompt = new EnterCategoryPrompt(sender, logger);
        prompt.Init();
        
        // Assert
        Assert.That(prompt.Text, Is.EqualTo("Enter a category (categories: abc, def)"));
        sender.Received().Send(Arg.Any<GetCategoriesQuery>());        
    }

    [Test]
    public void Should_Fail_Validation_When_Category_Is_Invalid()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var logger = Substitute.For<ILogger<EnterCategoryPrompt>>();
        sender.Send(Arg.Any<GetCategoriesQuery>())
            .Returns(new List<string>{ "abc", "def" });
        
        var prompt = new EnterCategoryPrompt(sender, logger)
        {
            Result = "invalid"
        };
        prompt.Init();
        
        // Act
        var validationResult = prompt.Validate();
        
        // Assert
        Assert.That(validationResult, Is.False);
    }
    
    [Test]
    public void Should_Fail_Validation_When_Category_Is_Empty()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var logger = Substitute.For<ILogger<EnterCategoryPrompt>>();
        sender.Send(Arg.Any<GetCategoriesQuery>())
            .Returns(new List<string>{ "abc", "def" });
        
        var prompt = new EnterCategoryPrompt(sender, logger)
        {
            Result = ""
        };
        prompt.Init();

        // Act
        var validationResult = prompt.Validate();
        
        // Assert
        Assert.That(validationResult, Is.False);
    }
    
    [Test]
    public void Should_Validate_Successfully_When_Category_Is_Valid()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var logger = Substitute.For<ILogger<EnterCategoryPrompt>>();
        sender.Send(Arg.Any<GetCategoriesQuery>())
            .Returns(new List<string>{ "abc", "def" });
        
        var prompt = new EnterCategoryPrompt(sender, logger)
        {
            Result = "def"
        };
        prompt.Init();
        
        // Act
        var validationResult = prompt.Validate();
        
        // Assert
        Assert.That(validationResult, Is.True);
    }
}