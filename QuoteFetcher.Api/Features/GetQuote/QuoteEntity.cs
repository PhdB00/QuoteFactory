namespace QuoteFetcher.Api.Features.GetQuote;

// INTENTIONAL: This entity has the same design issues as the Quote DTO:
// 1. Inconsistent property naming - mixes snake_case (category, icon_url) with PascalCase (Value),
//    violating C# naming conventions and creating inconsistent API responses
// 2. Misleading field name - "icon_url" suggests valid image URLs but stores non-existent URLs
public struct QuoteEntity
{
    public string category { get; init; }
    public string icon_url { get; init; }
    public string Value { get; init; }
}