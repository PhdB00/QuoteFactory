using NetArchTest.Rules;

namespace QuoteFetcher.ArchitectureTests.Api;

public class ApiTests : BaseTest
{
    [Test]
    public void ApiEndpoints_Should_Be_Static()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Endpoint")
            .Should()
            .BeStatic()
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }
}