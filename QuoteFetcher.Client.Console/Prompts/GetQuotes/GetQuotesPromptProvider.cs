using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace QuoteFetcher.Prompts.GetQuotes;

public interface IGetQuotesPromptProvider
{
    bool ExecutePromptSequence();
    int NumberOfQuotes { get; }
    string Category { get; }
}

internal sealed class GetQuotesPromptProvider : IGetQuotesPromptProvider
{
    private readonly IConsoleWrapper consoleWrapper;

    private readonly IPrompt numberPrompt;
    private readonly IPrompt enterCategoryPrompt;
    
    public GetQuotesPromptProvider(
        IConsoleWrapper consoleWrapper,
        List<IGetQuotesPrompt> getQuotesPrompts)
    {
        this.consoleWrapper = consoleWrapper;
        
        var prompts = getQuotesPrompts.Cast<IPrompt>()
            .ToFrozenDictionary(x => x.Id);
        
        enterCategoryPrompt = prompts["EnterCategory"];
        numberPrompt = prompts["Number"];
        var chooseCategoryPrompt = prompts["UseCategory"];
        
        numberPrompt.Next = () => chooseCategoryPrompt;
        
        chooseCategoryPrompt.Next = () => 
            chooseCategoryPrompt.Result.Equals("y", StringComparison.OrdinalIgnoreCase) ? 
                enterCategoryPrompt : null;
    }
    
    public bool ExecutePromptSequence()
    {
        var curr = numberPrompt; 
        while (curr != null)
        {
            consoleWrapper.WriteLine(curr.GetUserPrompt());
            if (curr.Style == PromptStyle.SingleCharacter)
            {
                curr.Result = consoleWrapper.ReadKey().KeyChar.ToString(); 
                consoleWrapper.WriteLine();
            }
            else
            {
                curr.Result = consoleWrapper.ReadLine().Trim();    
            }
            
            if (curr.Validate())
            {
                // ReSharper disable once ConstantConditionalAccessQualifier
                curr = curr.Next?.Invoke();
            }
            else
            {
                consoleWrapper.WriteLine("Sorry, that is an invalid response; please try again.");
            }
        }
        return true;
    }

    public string Category => enterCategoryPrompt.Result ?? string.Empty;
    
    public int NumberOfQuotes 
    { 
        get 
        {
            var value = int.TryParse(numberPrompt.Result, out var result) ? 
                result : 0;
            return value;
        }
    }
}
