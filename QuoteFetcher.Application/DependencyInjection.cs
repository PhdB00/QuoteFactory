using System.Reflection;
using FluentValidation;
using QuoteFetcher.Application.Abstractions.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using QuoteFetcher.Application.Quotes.GetQuotes;
using Refit;

namespace QuoteFetcher.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddOptions<QuoteApiSettings>()
            .BindConfiguration(QuoteApiSettings.ConfigSectionPath)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services
            .AddRefitClient<IQuoteApi>()
            .ConfigureHttpClient((provider, client) =>
                client.BaseAddress = 
                    new Uri(provider.GetRequiredService<IOptions<QuoteApiSettings>>().Value.ApiHost))
            .AddStandardResilienceHandler(builder =>
            {
                builder.Retry = new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 5,
                    Delay = TimeSpan.FromSeconds(1),
                    UseJitter = true,
                    BackoffType = DelayBackoffType.Exponential
                };
                builder.TotalRequestTimeout = new HttpTimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };
            });

        services.AddTransient<IValidator<GetQuotesStreamQuery>, GetQuotesRequestValidator>();
        services.AddTransient<IQuoteHashSet, QuoteHashSet>();

        services
            .AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(
                Assembly.GetExecutingAssembly());
            
        });
        services.AddMemoryCache();
        
        return services;
    }
}