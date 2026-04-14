namespace QuoteFetcher.Prompts.GetQuotes;

internal sealed class NumberOfQuotesPrompt : Prompt, IGetQuotesPrompt
{
    public NumberOfQuotesPrompt()
    {
        Id = "Number";
        Text = "How many quotes would you like? (1 - 9)";
        Validate = ValidateNumericRange;
    }
    
    private bool ValidateNumericRange()
    {
        var isNumeric = int.TryParse(Result, out var result);
        return isNumeric && result is >= 1 and <= 9;
    }
}