using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SecureLens.UI;
using SecureLens.Application;
using SecureLens.Application.Analysis.Calculators;
using SecureLens.Application.Analysis.Interfaces;
using SecureLens.Application.Services;
using SecureLens.Core.Models;
using SecureLens.Infrastructure.Factories;
using SecureLens.Infrastructure.Logging;

namespace SecureLens
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Byg konfiguration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config/appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("config/adminbyrequestsettings.json", optional: false, reloadOnChange: true)
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
