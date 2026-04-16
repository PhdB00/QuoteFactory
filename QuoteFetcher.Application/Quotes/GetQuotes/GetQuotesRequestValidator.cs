using FluentValidation;
using MediatR;
using QuoteFetcher.Application.Categories.GetCategories;

namespace QuoteFetcher.Application.Quotes.GetQuotes;

internal sealed class GetQuotesRequestValidator 
    : AbstractValidator<GetQuotesStreamQuery>
{
    private const string InvalidCategoryMessage = "Category is invalid.";

    public GetQuotesRequestValidator(ISender sender)
    {
        RuleFor(x => x.NumberOfQuotes)
            .InclusiveBetween(1, 9)
            .WithMessage("Number must be between 1 and 9");

        RuleFor(x => x.Category)
            .MustAsync(async (category, cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    return true;
                }

                try
                {
                    var categoriesResult = await sender.Send(new GetCategoriesQuery(), cancellationToken);
                    return categoriesResult.IsSuccess &&
                           categoriesResult.Value.Contains(category, StringComparer.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            })
            .WithMessage(InvalidCategoryMessage);
    }
}
