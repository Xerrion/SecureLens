using SecureLens.Application.Services.Interfaces;
using SecureLens.Core.Models;
using SecureLens.Infrastructure.Factories;
using SecureLens.Infrastructure.Logging;
using SecureLens.UI;

namespace SecureLens.Application;

public class ApplicationRunner(
    ILogger logger,
    UserInterface ui,
    SettingsManager settingsManager,
    ModeHandlerFactory factory)
{
    public async Task RunAsync()
    {
        settingsManager.InitializeSettings();

        logger.LogInfo("=== SecureLens Console Application ===");

        var mode = ui.GetMode();

        if (mode.Equals("cache", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInfo("You have chosen 'cache' mode. Loading from local JSON...");

            IModeHandler? cacheHandler = factory.CreateModeHandler("cache");
            await cacheHandler.ExecuteAsync();
        }
        else if (mode.Equals("online", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInfo("You have chosen 'online' mode.");

            var apiKey = ui.GetApiKey();
            
            if (!string.IsNullOrEmpty(apiKey))
            {
                IModeHandler? onlineHandler = factory.CreateModeHandler("online", apiKey);
                await onlineHandler.ExecuteAsync();
            }
            
            
        }
        else
        {
            logger.LogError("Invalid mode selected. Please choose 'cache' or 'online'.");
        }

        logger.LogInfo("\n=== Program Finished ===");
    }
}