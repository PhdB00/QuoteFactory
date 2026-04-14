using System.Net;
using System.Net.Http.Json;
using QuoteFetcher.Api.IntegrationTests.Infrastructure;

namespace QuoteFetcher.Api.IntegrationTests.Endpoints;

[TestFixture]
public class GetCategoriesEndpointTests
{
    private TestWebApplicationFactory factory = null!;
    private HttpClient client = null!;

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
    public async Task GetCategories_ReturnsOk_WithAllCategories()
    {
        // Act
        var response = await client.GetAsync("/quote_category");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var categories = await response.Content.ReadFromJsonAsync<string[]>();
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(categories, Is.Not.Null);
            Assert.That(categories!.Length, Is.EqualTo(9));
            Assert.That(categories, Does.Contain("animal"));
            Assert.That(categories, Does.Contain("celebrity"));
            Assert.That(categories, Does.Contain("food"));
            Assert.That(categories, Does.Contain("jazz"));
            Assert.That(categories, Does.Contain("money"));
            Assert.That(categories, Does.Contain("politics"));
            Assert.That(categories, Does.Contain("religion"));
            Assert.That(categories, Does.Contain("science"));
            Assert.That(categories, Does.Contain("sport"));
        }
    }

    [Test]
    public async Task GetCategories_ReturnsJsonArray()
    {
        // Act
        var response = await client.GetAsync("/quote_category");

        // Assert
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        var categories = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.That(categories, Is.Not.Null);
        Assert.That(categories, Is.InstanceOf<string[]>());
    }
}
