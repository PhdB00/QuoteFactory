var builder = WebApplication.CreateBuilder(args);

// Enable CORS for API communication
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Enable default files (must be before UseStaticFiles)
app.UseDefaultFiles();

// Enable static files
app.UseStaticFiles();

// Enable CORS
app.UseCors();

app.Run();
