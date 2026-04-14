using System.Text.Json.Serialization;

namespace QuoteFetcher.Application.Quotes.GetQuotes;

public class Quote
{
    [JsonPropertyName("icon_url")]
    public string IconUrl { get; init; } = string.Empty;
    
    [JsonPropertyName("Value")]
    public string Text { get; init; } = string.Empty;
}
