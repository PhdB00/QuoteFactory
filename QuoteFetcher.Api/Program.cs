using QuoteFetcher.Api.Features.GetCategories;
using QuoteFetcher.Api.Features.GetQuote;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddCategories();
builder.Services.AddQuotes("quotes.txt");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGetCategoriesEndpoint();
app.MapGetQuoteEndpoint();

await app.RunAsync();

// Make Program class accessible for integration tests
public partial class Program { }

