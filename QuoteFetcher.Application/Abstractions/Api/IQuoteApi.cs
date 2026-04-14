using QuoteFetcher.Application.Quotes.GetQuotes;
using Refit;

namespace QuoteFetcher.Application.Abstractions.Api;

public interface IQuoteApi
{
    [Get("/quote_category")]
    Task<List<string>> GetCategoriesAsync();
    
    [Get("/quote")]
    Task<Quote> GetQuoteAsync([Query] GetQuoteQueryParameters parameters);
}

public sealed class GetQuoteQueryParameters
{
    public required string Category { get; set; }
}