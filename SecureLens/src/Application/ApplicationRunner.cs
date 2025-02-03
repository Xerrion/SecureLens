using SecureLens.Application.Services.Interfaces;
using SecureLens.Core.Models;
using SecureLens.Infrastructure.Factories;
using SecureLens.Infrastructure.Logging;
using SecureLens.UI;

namespace SecureLens.Application;

public class ApplicationRunner
{
    private readonly ILogger _logger;
    private readonly UserInterface _ui;
    private readonly SettingsManager _settingsManager;
    private readonly ModeHandlerFactory _factory;

    public ApplicationRunner(
        ILogger logger,
        UserInterface ui,
        SettingsManager settingsManager,
        ModeHandlerFactory factory)
    {
        _logger = logger;
        _ui = ui;
        _settingsManager = settingsManager;
        _factory = factory;
    }

    public async Task RunAsync()
    {
        List<AdminByRequestSetting>? settings = _settingsManager.InitializeSettings();

        _logger.LogInfo("=== SecureLens Console Application ===");

        var mode = _ui.GetMode();

        if (mode.Equals("cache", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInfo("You have chosen 'cache' mode. Loading from local JSON...");

            IModeHandler? cacheHandler = _factory.CreateModeHandler("cache");
            await cacheHandler.ExecuteAsync();
        }
        else if (mode.Equals("online", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInfo("You have chosen 'online' mode.");

            var apiKey = _ui.GetApiKey();
            if (!string.IsNullOrEmpty(apiKey))
            {
                IModeHandler? onlineHandler = _factory.CreateModeHandler("online", apiKey);
                await onlineHandler.ExecuteAsync();
            }
        }
        else
        {
            _logger.LogError("Invalid mode selected. Please choose 'cache' or 'online'.");
        }

        _logger.LogInfo("\n=== Program Finished ===");
    }
}