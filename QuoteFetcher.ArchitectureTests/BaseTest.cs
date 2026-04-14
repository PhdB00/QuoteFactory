using System.Reflection;
using QuoteFetcher.Abstractions.Messaging;
using QuoteFetcher.Api.Features.GetCategories;
using QuoteFetcher.Application.Abstractions.Messaging;

namespace QuoteFetcher.ArchitectureTests;

public abstract class BaseTest
{
    protected static readonly Assembly ApiAssembly = typeof(GetCategoriesEndpoint).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(IQuery<>).Assembly;
    protected static readonly Assembly ConsoleAssembly = typeof(IMenuRequest<>).Assembly;
}