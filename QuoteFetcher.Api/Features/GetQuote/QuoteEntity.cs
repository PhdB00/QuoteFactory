namespace QuoteFetcher.Api.Features.GetQuote;

public struct QuoteEntity
{
    public string category { get; init; }
    public string icon_url { get; init; }
    public string Value { get; init; }
}