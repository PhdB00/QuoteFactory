// QuoteFetcher.Api/Features/GetCategories/CategoriesConfiguration.cs
namespace QuoteFetcher.Api.Features.GetCategories;

public static class CategoriesConfiguration
{
    private static readonly string[] Categories = 
    [
        "animal", "celebrity", "food", "jazz", "money", 
        "politics", "religion", "science", "sport"
    ];
    
    public static IServiceCollection AddCategories(this IServiceCollection services)
    {
        return services.AddSingleton(Categories);
    }
}