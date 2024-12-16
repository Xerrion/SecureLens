using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SecureLens.Logging;
using SecureLens.Services;
using SecureLens.UI;
using SecureLens.Factories;
using System.IO;
using SecureLens.Data;
using SecureLens.Data.Strategies;
using SecureLens.Data.Strategies.Interfaces;
using SecureLens.Analysis;

namespace SecureLens
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Byg konfiguration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("adminbyrequestsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Opsæt DI
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<ILogger, ConsoleLogger>()
                .AddSingleton<UserInterface>()
                .AddSingleton<SettingsManager>()
                .AddSingleton<List<AdminByRequestSetting>>(sp =>
                {
                    var settingsManager = sp.GetRequiredService<SettingsManager>();
                    return settingsManager.InitializeSettings();
                })
                .AddTransient<IOverallStatisticsCalculator, OverallStatisticsCalculator>()
                .AddTransient<IApplicationStatisticsCalculator, ApplicationStatisticsCalculator>()
                .AddTransient<ITerminalStatisticsCalculator, TerminalStatisticsCalculator>()
                .AddTransient<IUnusedAdGroupsCalculator, UnusedAdGroupsCalculator>()
                .AddTransient<Analyzer>() 
                .AddTransient<CacheModeHandler>()
                .AddTransient<OnlineModeHandler>()
                .AddSingleton<ModeHandlerFactory>()
                .AddSingleton<ApplicationRunner>()
                .BuildServiceProvider();

            // Resolve ApplicationRunner og kør
            var runner = serviceProvider.GetService<ApplicationRunner>();
            if (runner != null)
            {
                await runner.RunAsync();
            }
            else
            {
                Console.WriteLine("Failed to start the application.");
            }
        }
    }
}
