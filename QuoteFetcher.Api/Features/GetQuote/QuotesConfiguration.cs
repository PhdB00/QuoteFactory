namespace QuoteFetcher.Api.Features.GetQuote;

public static class QuotesConfiguration
{
    // This default value is intentionally invalid to mirror the original tasks smuggling of the invalid data
    // in with the response from the API.
    private const string DefaultIconUrl = "https://localhost.invalid.com/xxxxxxx.jpg";

    public static IServiceCollection AddQuotes(this IServiceCollection services, string filePath)
    {
        var quotes = LoadQuotesFromFile(filePath);
        return services.AddSingleton(quotes);
    }

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