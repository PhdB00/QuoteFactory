using QuoteFetcher.Application.Abstractions.Messaging;

namespace QuoteFetcher.Application.Quotes.GetQuotes;

public sealed record GetQuotesStreamQuery(int NumberOfQuotes, string Category = "") 
    : IStreamQuery<string>;    