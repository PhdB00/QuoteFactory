using System.Net;
using System.Net.Http.Json;
using QuoteFetcher.Api.Features.GetQuote;
using QuoteFetcher.Api.IntegrationTests.Infrastructure;

namespace QuoteFetcher.Api.IntegrationTests.Endpoints;

[TestFixture]
public class GetQuoteEndpointTests
{
    private TestWebApplicationFactory factory = null!;
    private HttpClient client = null!;

    private static readonly string[] ExpectedAnimalQuotes =
    [
        "Animal quote 1",
        "Animal quote 2",
        "Animal quote 3"
    ];

    [SetUp]
    public void Setup()
    {
        factory = new TestWebApplicationFactory();
        client = factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        client.Dispose();
        factory.Dispose();
    }

    [Test]
    public async Task GetQuote_WithoutCategory_ReturnsOk_WithRandomQuote()
    {
        // Act
        var response = await client.GetAsync("/quote");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var quote = await response.Content.ReadFromJsonAsync<Quote>();
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(quote.Value, Is.Not.Null);
            Assert.That(quote.Value, Is.Not.Empty);
            Assert.That(quote.icon_url, Is.Not.Null);
            Assert.That(quote.icon_url, Is.Not.Empty);
        }
    }

    [Test]
    public async Task GetQuote_WithoutCategory_ReturnsValidQuoteStructure()
    {
        // Act
        var response = await client.GetAsync("/quote");

        // Assert
        var quote = await response.Content.ReadFromJsonAsync<Quote>();
        // Verify icon_url is present and not empty (actual value may vary based on data source)
        using (Assert.EnterMultipleScope())
        {
            Assert.That(quote.icon_url, Is.Not.Null);
            Assert.That(quote.icon_url, Is.Not.Empty);
            Assert.That(quote.Value, Is.Not.Empty);
        }
    }

    [Test]
    public async Task GetQuote_WithValidCategory_ReturnsOk_WithQuoteFromCategory()
    {
        // Act
        var response = await client.GetAsync("/quote?category=animal");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var quote = await response.Content.ReadFromJsonAsync<Quote>();
        Assert.That(quote.Value, Is.Not.Null);
        Assert.That(ExpectedAnimalQuotes, Does.Contain(quote.Value));
    }

    [Test]
    public async Task GetQuote_WithMultipleCallsSameCategory_ReturnsDifferentQuotes()
    {
        // Arrange
        var quotes = new HashSet<string>();
        const int numberOfCalls = 20;

        // Act
        for (int i = 0; i < numberOfCalls; i++)
        {
            var response = await client.GetAsync("/quote?category=sport");
            var quote = await response.Content.ReadFromJsonAsync<Quote>();
            quotes.Add(quote.Value);
        }

        // Assert - With 4 sport quotes and 20 calls, we should get at least 2 different quotes
        Assert.That(quotes.Count, Is.GreaterThanOrEqualTo(2),
            "Expected to receive multiple different quotes across multiple calls");
    }

    [Test]
    public async Task GetQuote_WithInvalidCategory_ReturnsNotFound()
    {
        // Act
        var response = await client.GetAsync("/quote?category=nonexistent");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetQuote_WithEmptyCategory_TreatsAsNoCategory()
    {
        // Act
        var response = await client.GetAsync("/quote?category=");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var quote = await response.Content.ReadFromJsonAsync<Quote>();
        Assert.That(quote.Value, Is.Not.Null);
        Assert.That(quote.Value, Is.Not.Empty);
    }

    [Test]
    public async Task GetQuote_WithCaseVariation_IsCaseSensitive()
    {
        // Act - lowercase should work
        var responseLower = await client.GetAsync("/quote?category=animal");

        // uppercase should not find matches (case sensitive)
        var responseUpper = await client.GetAsync("/quote?category=ANIMAL");

        // Assert
        Assert.That(responseLower.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseUpper.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
