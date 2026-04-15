namespace QuoteFetcher.Api.Features.GetCategories;

public interface ICategoryProvider
{
    string[] GetCategories();
}