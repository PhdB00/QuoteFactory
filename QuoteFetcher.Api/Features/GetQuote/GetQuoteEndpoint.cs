namespace QuoteFetcher.Api.Features.GetQuote;

public static class GetQuoteEndpoint
{
    public static void MapGetQuoteEndpoint(this IEndpointRouteBuilder app)
    {
        // INTENTIONAL: Multiple issues in this endpoint that mirror the original assessment API:
        // 1. No pagination support - returns only a single quote per request, forcing clients
        //    to make multiple sequential calls for bulk operations (inefficient)
        // 2. No input validation - category parameter is not validated against known categories,
        //    invalid categories simply return 404 without helpful error messages
        // 3. Random selection guarantees duplicates - pure random selection with no duplicate
        //    prevention means multiple requests will often return the same quotes
        app.MapGet("/quote", (string? category, List<QuoteEntity> quotes) =>
        {
            var filteredQuotes = string.IsNullOrEmpty(category)
                ? quotes
                : quotes.Where(q => q.category == category).ToList();

            if (filteredQuotes.Count == 0)
                return Results.NotFound();
            
            var selectedQuote = filteredQuotes[Random.Shared.Next(filteredQuotes.Count)];

            var quote = new Quote
            {
                icon_url = selectedQuote.icon_url,
                Value = selectedQuote.Value
            };

            return Results.Ok(quote);
        })
        .WithName("GetQuote");
    }
}
