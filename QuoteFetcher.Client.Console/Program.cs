using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using QuoteFetcher;
using QuoteFetcher.Application;
using QuoteFetcher.MenuSystem;
using QuoteFetcher.MenuSystem.GetCategories;
using QuoteFetcher.MenuSystem.Invalid;
using QuoteFetcher.MenuSystem.Quit;
using QuoteFetcher.MenuSystem.ShowMenu;
using Microsoft.Extensions.Options;
using QuoteFetcher.MenuSystem.GetQuotes;
using QuoteFetcher.Prompts.GetQuotes;

try
{
    var builder = CreateHostBuilder(args);
    builder = ConfigureServices(builder);

    await builder.RunConsoleAsync();
}
catch (OptionsValidationException e)
{
    Console.WriteLine("");
    Console.WriteLine("There is an error with the configuration settings. Please verify options and try again.");
    foreach (var error in e.Failures)
    {
        Console.WriteLine(error);    
    }
}

return;

static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args);
}

static IHostBuilder ConfigureServices(IHostBuilder builder)
{
    return builder
        .ConfigureServices((_, services) =>
        {
            services.AddTransient<IGetQuotesPrompt, NumberOfQuotesPrompt>();
            services.AddTransient<IGetQuotesPrompt, UseCategoryPrompt>();
            services.AddTransient<IGetQuotesPrompt, EnterCategoryPrompt>();
            services.AddTransient(provider => 
                provider.GetServices<IGetQuotesPrompt>().ToList());
            services.AddTransient<IGetQuotesPromptProvider, GetQuotesPromptProvider>(); 
            
            services.AddTransient<IMenuItem, ShowMenuItem>();
            services.AddTransient<IMenuItem, GetCategoriesMenuItem>(); 
            services.AddTransient<IMenuItem, GetQuotesMenuItem>();
            services.AddTransient<IMenuItem, QuitMenuItem>();
            services.AddTransient<IMenuItem, InvalidMenuItem>();
            services.AddSingleton(provider => 
                provider.GetServices<IMenuItem>().ToList());

            services.AddSingleton<IMenuItems, MenuItems>();
            services.AddTransient<IMenuRenderer, MenuRenderer>();
            services.AddTransient<IMenuProvider, MenuProvider>();
            
            services.AddTransient<IConsoleWrapper, ConsoleWrapper>();
            services.AddHostedService<ConsoleHostedService>();
            
            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssembly(Assembly.GetEntryAssembly());
            });
            
            services.AddApplication();
        });
}