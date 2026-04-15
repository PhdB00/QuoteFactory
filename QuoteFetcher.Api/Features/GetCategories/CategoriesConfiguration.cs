// QuoteFetcher.Api/Features/GetCategories/CategoriesConfiguration.cs
namespace QuoteFetcher.Api.Features.GetCategories;

public static class CategoriesConfiguration
{
    public static IServiceCollection AddCategories(this IServiceCollection services)
    {
        return services.AddSingleton<ICategoryProvider, CategoryProvider>();
    }
}