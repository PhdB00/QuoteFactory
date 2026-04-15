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

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

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
    // Prevent clickjacking
    context.Response.Headers["X-Frame-Options"] = "DENY";

    // Prevent MIME sniffing
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

    // Referrer policy
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    // Content Security Policy
    var apiBaseUrl = context.RequestServices.GetRequiredService<IConfiguration>()["ApiBaseUrl"] ?? "http://localhost:5074";
    if (apiBaseUrl.Contains(";"))
    {
        apiBaseUrl = apiBaseUrl.Split(';')[0].Trim();
    }
    context.Response.Headers["Content-Security-Policy"] =
        $"default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; connect-src 'self' {apiBaseUrl}; object-src 'none'; base-uri 'self'; frame-ancestors 'none';";

    await next(context);
});

// Enable default files (must be before UseStaticFiles)
app.UseDefaultFiles();

// Enable static files
app.UseStaticFiles();

// Enable CORS
app.UseCors();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// Configuration endpoint for frontend
app.MapGet("/api/config", (IConfiguration config) =>
{
    var apiBaseUrl = config["ApiBaseUrl"] ?? "http://localhost:5074";

    // Parse the URL to ensure we return a clean base URL
    if (apiBaseUrl.Contains(";"))
    {
        apiBaseUrl = apiBaseUrl.Split(';')[0].Trim();
    }

    return Results.Json(new
    {
        apiBaseUrl = apiBaseUrl
    });
});

await app.RunAsync();
