using System.Net;
using System.Net.Http.Json;
using QuoteFetcher.Api.Features.GetQuote;
using QuoteFetcher.Api.IntegrationTests.Infrastructure;

namespace QuoteFetcher.Api.IntegrationTests.Configuration;

[TestFixture]
public class QuotesConfigurationTests
{
    [Test]
    public void Application_WithValidQuotesFile_StartsSuccessfully()
    {
        // Arrange & Act
        using var factory = new TestWebApplicationFactory("TestData/test-quotes.txt");
        using var client = factory.CreateClient();

        // Assert - If we can make a request, the app started successfully
        var response = client.GetAsync("/quote_category").Result;
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public void Application_WithMissingQuotesFile_ThrowsFileNotFoundException()
    {
        // Arrange & Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
        {
            using var factory = new TestWebApplicationFactory("nonexistent-file.txt");
            using var client = factory.CreateClient();
            // Trigger the service resolution which will throw
            _ = client.GetAsync("/quote").Result;
        });
    }

    [Test]
    public void Application_WithMalformedQuotesFile_ThrowsInvalidDataException()
    {
        // Arrange & Act & Assert
        Assert.Throws<InvalidDataException>(() =>
        {
            using var factory = new TestWebApplicationFactory("TestData/malformed-quotes.txt");
            using var client = factory.CreateClient();
            // Trigger the service resolution which will throw
            _ = client.GetAsync("/quote").Result;
        });
    }

    [Test]
    public async Task Application_WithEmptyQuotesFile_LoadsEmptyList()
    {
        // Arrange
        var emptyFilePath = "TestData/empty-quotes.txt";
        await File.WriteAllTextAsync(emptyFilePath, string.Empty);

        try
        {
            using var factory = new TestWebApplicationFactory(emptyFilePath);
            using var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/quote");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
        finally
        {
            if (File.Exists(emptyFilePath))
            {
                File.Delete(emptyFilePath);
            }
        }
    }

    [Test]
    public async Task Application_LoadsQuotesWithCorrectParsing()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory("TestData/test-quotes.txt");
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/quote?category=animal");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var quote = await response.Content.ReadFromJsonAsync<Quote>();

        // Verify category was extracted correctly
        Assert.That(new[]
            {
                "Animal quote 1", "Animal quote 2", "Animal quote 3"
            },
            Does.Contain(quote.Value));

        // Verify icon_url is present and not empty (actual value may vary based on data source)
        Assert.That(quote.icon_url, Is.Not.Null);
        Assert.That(quote.icon_url, Is.Not.Empty);

        // Verify value is trimmed (no leading/trailing whitespace)
        Assert.That(quote.Value, Is.EqualTo(quote.Value.Trim()));
    }
}
