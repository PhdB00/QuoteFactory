using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace QuoteFetcher.Web.HealthChecks;

public sealed class ApiDependencyHealthCheck(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var apiBaseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5074";
        if (apiBaseUrl.Contains(';'))
        {
            apiBaseUrl = apiBaseUrl.Split(';')[0].Trim();
        }

        if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var baseUri))
        {
            return HealthCheckResult.Unhealthy("ApiBaseUrl is invalid.");
        }

        var probeUri = new Uri(baseUri, "/quote_category");

        try
        {
            using var client = httpClientFactory.CreateClient("ReadinessChecks");
            using var request = new HttpRequestMessage(HttpMethod.Get, probeUri);
            using var response = await client.SendAsync(request, cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("QuoteFetcher.Api dependency is reachable.")
                : HealthCheckResult.Unhealthy(
                    $"QuoteFetcher.Api dependency returned status code {(int)response.StatusCode}.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "QuoteFetcher.Api dependency check failed.",
                ex);
        }
    }
}
