using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using QuoteFetcher.Api.Features.GetQuote;

namespace QuoteFetcher.Api.IntegrationTests.Infrastructure;

public class TestWebApplicationFactory(
    string quotesFilePath = "TestData/test-quotes.txt")
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing quotes registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(List<QuoteEntity>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add test quotes
            services.AddQuotes(quotesFilePath);
        });

        builder.UseEnvironment("Testing");
    }
}
