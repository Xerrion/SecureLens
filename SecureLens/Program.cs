using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SecureLens.Logging;
using SecureLens.Services;
using SecureLens.UI;
using SecureLens.Factories;
using System.IO;

namespace SecureLens
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("adminbyrequestsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Setup DI
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<ILogger, ConsoleLogger>()
                .AddSingleton<UserInterface>()
                .AddSingleton<SettingsManager>()
                .AddSingleton(sp => 
                {
                    var settingsManager = sp.GetRequiredService<SettingsManager>();
                    return settingsManager.InitializeSettings();
                }) 
                .AddTransient<CacheModeHandler>()
                .AddSingleton<ModeHandlerFactory>()
                .AddSingleton<ApplicationRunner>()
                .BuildServiceProvider();

            // Resolve ApplicationRunner and run
            var runner = serviceProvider.GetService<ApplicationRunner>();
            if (runner != null)
            {
                await runner.RunAsync();
            }
        }
    }
}