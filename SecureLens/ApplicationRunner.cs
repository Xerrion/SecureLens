// ApplicationRunner.cs
using SecureLens.UI;
using SecureLens.Services;
using SecureLens.Logging;
using SecureLens.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace SecureLens
{
    public class ApplicationRunner
    {
        private readonly ILogger _logger;
        private readonly UserInterface _ui;
        private readonly SettingsManager _settingsManager;
        private readonly ModeHandlerFactory _factory;

        public ApplicationRunner(ILogger logger, UserInterface ui, SettingsManager settingsManager, ModeHandlerFactory factory)
        {
            _logger = logger;
            _ui = ui;
            _settingsManager = settingsManager;
            _factory = factory;
        }

        public async Task RunAsync()
        {
            var settings = _settingsManager.InitializeSettings();

            _logger.LogInfo("=== SecureLens Console Application ===");

            string mode = _ui.GetMode();

            if (mode == "cache")
            {
                _logger.LogInfo("You have chosen 'cache' mode. Loading from local JSON...");

                var cacheHandler = _factory.CreateModeHandler("cache");
                await cacheHandler.ExecuteAsync();
            }
            else if (mode == "online")
            {
                _logger.LogInfo("You have chosen 'online' mode.");

                string apiKey = _ui.GetApiKey();

                var onlineHandler = _factory.CreateModeHandler("online", apiKey);
                await onlineHandler.ExecuteAsync();
            }
            else
            {
                _logger.LogError("Invalid mode selected. Please choose 'cache' or 'online'.");
            }

            _logger.LogInfo("\n=== Program Finished ===");
        }
    }
}