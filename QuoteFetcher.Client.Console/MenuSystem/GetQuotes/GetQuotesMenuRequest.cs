using QuoteFetcher.Abstractions.Messaging;

namespace QuoteFetcher.MenuSystem.GetQuotes;

internal sealed record GetQuotesMenuRequest : IMenuRequest<int>;