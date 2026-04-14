using QuoteFetcher.Abstractions.Messaging;

namespace QuoteFetcher.MenuSystem.GetCategories;

internal sealed record GetCategoriesMenuRequest : IMenuRequest<int>;