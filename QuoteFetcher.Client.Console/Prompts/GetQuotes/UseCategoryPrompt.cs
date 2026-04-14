using System;

namespace QuoteFetcher.Prompts.GetQuotes;

internal sealed class UseCategoryPrompt : Prompt, IGetQuotesPrompt
{
    public UseCategoryPrompt()
    {
        Id = "UseCategory";
        Text = "Choose a category? (y or n)";
        Validate = ValidateYesOrNo;
    }
    
    private bool ValidateYesOrNo()
    {
        return Result.Equals("y", StringComparison.OrdinalIgnoreCase) || 
               Result.Equals("n", StringComparison.OrdinalIgnoreCase);
    }
}