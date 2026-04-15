using QuoteFetcher.Api.Features.GetCategories;
using QuoteFetcher.Api.Features.GetQuote;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Add CORS for web application
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5001", "https://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddCategories();
builder.Services.AddQuotes("quotes.txt");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors();

app.MapGetCategoriesEndpoint();
app.MapGetQuoteEndpoint();

await app.RunAsync();

// Make Program class accessible for integration tests
public partial class Program { }

