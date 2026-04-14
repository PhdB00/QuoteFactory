using QuoteFetcher.Prompts.GetQuotes;

namespace QuoteFetcher.UnitTests.Prompts.GetQuotes;

[TestFixture]
public class NumberOfQuotesPromptTests
{
    private static readonly string[] InvalidResults =
    [
        "0",
        "-1",
        "1.1",
        "10",
        "a",
        ""
    ];
    
    [Test]
    [TestCaseSource(nameof(InvalidResults))]
    public void Should_Fail_Validation(string invalid)
    {
        // Arrange
        var prompt = new NumberOfQuotesPrompt
        {
            Result = invalid
        };

        // Act
        var result = prompt.Validate();
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    private static readonly string[] ValidResults =
    [
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9"
    ];
    
    [Test]
    [TestCaseSource(nameof(ValidResults))]
    public void Should_Pass_Validation(string valid)
    {
        // Arrange
        var prompt = new NumberOfQuotesPrompt
        {
            Result = valid
        };

        // Act
        var result = prompt.Validate();
        
        // Assert
        Assert.That(result, Is.True);
    }
}