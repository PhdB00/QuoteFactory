namespace QuoteFetcher.Api.Features.GetQuote;

// INTENTIONAL: This DTO has multiple design issues that replicate the original assessment API:
// 1. Inconsistent property naming - mixes snake_case (icon_url) with PascalCase (Value),
//    violating C# naming conventions and API design consistency
// 2. Misleading field name - "icon_url" suggests valid image URLs but actually contains
//    non-existent URLs that don't resolve to actual images
public struct Quote
{
    public string icon_url { get; init; }
    public string Value { get; init; }
}