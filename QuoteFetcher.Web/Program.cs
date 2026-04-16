using QuoteFetcher.Web.Configuration;
using QuoteFetcher.Web.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Enable CORS for API communication
var allowedOrigins = builder.Configuration
                         .GetSection("AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:5001" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHttpClient("ReadinessChecks", client =>
{
    client.Timeout = TimeSpan.FromSeconds(3);
});

// Add health checks with explicit liveness/readiness tagging
builder.Services.AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy("Web process is alive."),
        tags: new[] { "live" })
    .AddCheck<ApiDependencyHealthCheck>(
        "quote-api",
        tags: new[] { "ready" });

builder.Services
    .AddOptions<SecurityHeaderOptions>()
    .Bind(builder.Configuration.GetSection(SecurityHeaderOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(
        options => SecurityHeaderOptions.IsValidXFrameOptions(options.XFrameOptions),
        "SecurityHeaders:XFrameOptions must be DENY or SAMEORIGIN.")
    .Validate(
        options => SecurityHeaderOptions.IsValidXContentTypeOptions(options.XContentTypeOptions),
        "SecurityHeaders:XContentTypeOptions must be nosniff.")
    .Validate(
        options => SecurityHeaderOptions.IsValidReferrerPolicy(options.ReferrerPolicy),
        "SecurityHeaders:ReferrerPolicy has an invalid value.")
    .Validate(
        options => SecurityHeaderOptions.IsValidContentSecurityPolicy(options.ContentSecurityPolicy),
        "SecurityHeaders:ContentSecurityPolicy is missing required directives.")
    .ValidateOnStart();

var app = builder.Build();
var securityHeaders = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecurityHeaderOptions>>().Value;

// Environment-specific configuration
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Global exception handling middleware
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "An unexpected error occurred.",
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        });
    });
    
    // HSTS for production
    app.UseHsts();
}

// Security headers middleware
app.Use(async (HttpContext context, RequestDelegate next) =>
{
    context.Response.Headers["X-Frame-Options"] = securityHeaders.XFrameOptions;
    context.Response.Headers["X-Content-Type-Options"] = securityHeaders.XContentTypeOptions;
    context.Response.Headers["Referrer-Policy"] = securityHeaders.ReferrerPolicy;
    context.Response.Headers["Content-Security-Policy"] = securityHeaders.ContentSecurityPolicy;

    await next(context);
});

// Enable default files (must be before UseStaticFiles)
app.UseDefaultFiles();

// Enable static files
app.UseStaticFiles();

// Enable CORS
app.UseCors();

// Health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// Configuration endpoint for frontend
app.MapGet("/api/config", (IConfiguration config) =>
{
    var apiBaseUrl = config["ApiBaseUrl"] ?? "http://localhost:5074";
    var bubbleVfxSection = config.GetSection("BubbleVfx");

    // Parse the URL to ensure we return a clean base URL
    if (apiBaseUrl.Contains(";"))
    {
        apiBaseUrl = apiBaseUrl.Split(';')[0].Trim();
    }

    var preset = bubbleVfxSection["Preset"] ?? "arcade_punchy";
    var seed = bubbleVfxSection.GetValue<int?>("Seed") ?? 1337;
    var allowAudioOverlap = bubbleVfxSection.GetValue<bool?>("AllowAudioOverlap") ?? false;
    var respawnDelayMs = bubbleVfxSection.GetValue<int?>("RespawnDelayMs") ?? 550;
    var explosionDurationMs = bubbleVfxSection.GetValue<int?>("ExplosionDurationMs") ?? 600;
    var clickFeedbackDurationMs = bubbleVfxSection.GetValue<int?>("ClickFeedbackDurationMs") ?? 100;

    return Results.Json(new
    {
        apiBaseUrl = apiBaseUrl,
        bubbleVfx = new
        {
            preset,
            seed,
            allowAudioOverlap,
            respawnDelayMs,
            explosionDurationMs,
            clickFeedbackDurationMs
        }
    });
});

await app.RunAsync();
