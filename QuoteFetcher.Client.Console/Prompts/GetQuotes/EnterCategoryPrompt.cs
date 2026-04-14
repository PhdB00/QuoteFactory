using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using QuoteFetcher.Application.Categories.GetCategories;

namespace QuoteFetcher.Prompts.GetQuotes;

internal sealed class EnterCategoryPrompt : Prompt, IGetQuotesPrompt
{
    private readonly ILogger<EnterCategoryPrompt> logger;
    private ISender sender { get; }
    private IReadOnlyList<string> categories;
    private bool initialized;
    
    public EnterCategoryPrompt(
        ISender sender,
        ILogger<EnterCategoryPrompt> logger)
    {
        this.logger = logger;
        this.sender = sender;
        
        Id = "EnterCategory";
        Text = "Enter a category";
        Style = PromptStyle.MultiCharacter; 
        Validate = ValidateCategory;
        Init = Initialize;
    }
    
    private bool Initialize()
    {
        if (initialized)
        {
            return true;
        }
        
        var categoriesTask = sender.Send(new GetCategoriesQuery());
        while (!categoriesTask.IsCompleted)
        {
            logger.LogDebug("Waiting for Categories to be returned before prompting");
            categoriesTask.Wait(1000);
        }
        
        if (categoriesTask.IsCompletedSuccessfully)
        {
            categories = categoriesTask.Result.Value;
            SetPromptTextWithCategories();    
        }

        initialized = true;
        
        return true;
    }
    
    private void SetPromptTextWithCategories()
    {
        if (!categories.Any())
        {
            return;
        }
        
        var sb = new StringBuilder("Enter a category (categories: ");
        sb.AppendJoin(", ", categories.Select(x => x));
        sb.Append(')');
        Text = sb.ToString();
    }
    
    private bool ValidateCategory()
    {
        if (string.IsNullOrEmpty(Result))
        {
            return false;
        }
        
        if (!categories.Any() || 
            categories.Contains(Result, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }
        
        return false;
    }
}