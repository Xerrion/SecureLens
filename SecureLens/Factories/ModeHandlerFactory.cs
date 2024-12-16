// ModeHandlerFactory.cs

using Microsoft.Extensions.DependencyInjection;
using SecureLens.Logging;
using SecureLens.Services;

namespace SecureLens.Factories
{
    public class ModeHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly List<AdminByRequestSetting> _settings;

        public ModeHandlerFactory(IServiceProvider serviceProvider, ILogger logger, List<AdminByRequestSetting> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings;
        }

        public IModeHandler CreateModeHandler(string mode, string apiKey = "")
        {
            if (mode == "cache")
            {
                return _serviceProvider.GetService<CacheModeHandler>()!;
            }
            else if (mode == "online")
            {
                return new OnlineModeHandler(_logger, _settings, apiKey);
            }
            else
            {
                throw new ArgumentException("Invalid mode");
            }
        }
    }
}