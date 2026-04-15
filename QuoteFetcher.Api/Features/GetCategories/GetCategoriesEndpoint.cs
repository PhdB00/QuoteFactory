namespace QuoteFetcher.Api.Features.GetCategories;

public static class GetCategoriesEndpoint
{
    public static void MapGetCategoriesEndpoint(this IEndpointRouteBuilder app)
    {
        // INTENTIONAL: Non-RESTful endpoint naming - uses "/quote_category" instead of conventional
        // REST naming like "/categories" or "/quote-categories". This violates REST conventions
        // and API design best practices.
        app.MapGet("/quote_category", (ICategoryProvider categoryProvider) =>
        {
            var categories = categoryProvider.GetCategories();
            return Results.Ok(categories);
        });
    }
}