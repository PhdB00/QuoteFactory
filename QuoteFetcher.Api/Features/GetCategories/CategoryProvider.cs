namespace QuoteFetcher.Api.Features.GetCategories;

public class CategoryProvider : ICategoryProvider
{
    private static readonly string[] Categories = 
    [
        "animal", "celebrity", "food", "jazz", "money", 
        "politics", "religion", "science", "sport"
    ];
    
    public string[] GetCategories() => Categories;
}