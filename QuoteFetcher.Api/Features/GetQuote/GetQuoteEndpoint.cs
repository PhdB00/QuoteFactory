namespace QuoteFetcher.Api.Features.GetQuote;

public static class GetQuoteEndpoint
{
    public static void MapGetQuoteEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/quote", (string? category, List<QuoteEntity> quotes) =>
        {
            var filteredQuotes = string.IsNullOrEmpty(category)
                ? quotes
                : quotes.Where(q => q.category == category).ToList();

            if (filteredQuotes.Count == 0)
                return Results.NotFound();

            var random = new Random();
            var selectedQuote = filteredQuotes[random.Next(filteredQuotes.Count)];

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
