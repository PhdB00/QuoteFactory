namespace QuoteFetcher.Api.Features.GetQuote;

public static class QuotesConfiguration
{
    // INTENTIONAL: Invalid/non-existent image URLs - this URL points to images that do not exist,
    // replicating the original assessment API's behavior. Client applications cannot display these
    // images, creating a misleading API contract.
    private const string DefaultIconUrl = "https://localhost.invalid.com/xxxxxxx.jpg";

    public static IServiceCollection AddQuotes(this IServiceCollection services, string filePath)
    {
        var quotes = LoadQuotesFromFile(filePath);
        return services.AddSingleton(quotes);
    }

    // INTENTIONAL: Limited dataset / insufficient quotes - the quotes.txt file contains a limited
    // number of quotes per category (e.g., only 20 animal quotes, 6 celebrity quotes, 3 sport quotes).
    // This makes it impossible to satisfy large requests for unique quotes and creates high
    // duplicate probability when clients request multiple quotes.
    private static List<QuoteEntity> LoadQuotesFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Quotes file not found at: {filePath}");
        }

        return File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line =>
            {
                var parts = line.Split('|', 2);
                if (parts.Length != 2)
                {
                    throw new InvalidDataException($"Invalid quote format: {line}");
                }

                return new QuoteEntity
                {
                    category = parts[0].Trim(),
                    Value = parts[1].Trim(),
                    icon_url = DefaultIconUrl
                };
            })
            .ToList();
    }
}