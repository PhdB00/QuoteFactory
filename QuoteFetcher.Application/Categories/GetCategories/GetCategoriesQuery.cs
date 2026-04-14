using QuoteFetcher.Application.Abstractions.Messaging;

namespace QuoteFetcher.Application.Categories.GetCategories;

public sealed record GetCategoriesQuery : IQuery<IReadOnlyList<string>>;