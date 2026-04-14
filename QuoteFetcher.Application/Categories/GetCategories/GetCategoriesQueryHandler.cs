using QuoteFetcher.Application.Abstractions;
using QuoteFetcher.Application.Abstractions.Api;
using QuoteFetcher.Application.Abstractions.Messaging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuoteFetcher.Application.Categories.GetCategories;

internal sealed class GetCategoriesQueryHandler(
    ILogger<GetCategoriesQueryHandler> logger,
    IQuoteApi api,
    IMemoryCache cache,
    IOptions<QuoteApiSettings> settings)
    : IQueryHandler<GetCategoriesQuery, IReadOnlyList<string>>
{ 
    public async Task<Result<IReadOnlyList<string>>> Handle(GetCategoriesQuery query, 
        CancellationToken cancellationToken)
    {
        return await cache.GetOrCreateAsync(
            "categories",
            async _ =>
            {
                logger.LogDebug("Requesting categories from Api");
                return await api.GetCategoriesAsync();
            }
            , new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(relative: TimeSpan.FromMinutes(settings.Value.CacheExpireAfterMinutes))
                .RegisterPostEvictionCallback((_, _, _, _) =>
                    {
                        logger.LogDebug("Categories evicted from cache");
                    })
            );
    }
}