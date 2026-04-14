using QuoteFetcher.Prompts.GetQuotes;

namespace QuoteFetcher.UnitTests.Prompts.GetQuotes;

[TestFixture]
public class UseCategoryPromptTests
{
    [TestCase("")]
    [TestCase("yes")]
    [TestCase("no")]
    public void Should_Fail_Validation(string invalid)
    {
        // Arrange
        var prompt = new UseCategoryPrompt
        {
            Result = invalid
        };

        // Act
        var result = prompt.Validate();
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    [TestCase("Y")]
    [TestCase("y")]
    [TestCase("n")]
    [TestCase("N")]
    public void Should_Pass_Validation(string valid)
    {
        // Arrange
        var prompt = new UseCategoryPrompt
        {
            Result = valid
        };

        // Act
        var result = prompt.Validate();
        
        // Assert
        Assert.That(result, Is.True);
    }
}