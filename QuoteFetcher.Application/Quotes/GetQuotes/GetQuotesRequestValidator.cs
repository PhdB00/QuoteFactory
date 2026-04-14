using FluentValidation;
using MediatR;
using QuoteFetcher.Application.Categories.GetCategories;

namespace QuoteFetcher.Application.Quotes.GetQuotes;

internal sealed class GetQuotesRequestValidator 
    : AbstractValidator<GetQuotesStreamQuery>
{
    public GetQuotesRequestValidator(ISender sender)
    {
        RuleFor(x => x.NumberOfQuotes)
            .InclusiveBetween(1, 9)
            .WithMessage("Number must be between 1 and 9");
        
        var getCategories = sender.Send(new GetCategoriesQuery());
        getCategories.Wait(1000);

        if (!getCategories.IsCompletedSuccessfully)
        {
            throw new InvalidOperationException("Failed to retrieve categories");
        }
        
        var categories = getCategories.Result.Value;
        RuleFor(x => x.Category)
            .Empty()
            .Unless(x => categories.Contains(x.Category, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Category must be one of the following: {string.Join(", ", categories)}");

    }
}