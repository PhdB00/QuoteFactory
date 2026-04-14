namespace QuoteFetcher.Api.Features.GetCategories;

public static class GetCategoriesEndpoint
{
    public static void MapGetCategoriesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/quote_category", () =>
        {
            var categories = app.ServiceProvider.GetRequiredService<string[]>();
            return Results.Ok(categories);
        });
    }
}