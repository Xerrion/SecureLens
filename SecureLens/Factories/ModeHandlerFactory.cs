using Microsoft.Extensions.DependencyInjection;
using SecureLens.Logging;
using SecureLens.Services;
using Microsoft.Extensions.Configuration;
using System;

namespace SecureLens.Factories
{
    public class ModeHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ModeHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IModeHandler CreateModeHandler(string mode, string apiKey = "")
        {
            if (mode.Equals("cache", StringComparison.OrdinalIgnoreCase))
            {
                return _serviceProvider.GetRequiredService<CacheModeHandler>();
            }
            else if (mode.Equals("online", StringComparison.OrdinalIgnoreCase))
            {
                // OnlineModeHandler kræver en API-nøgle, så opret den manuelt
                return ActivatorUtilities.CreateInstance<OnlineModeHandler>(_serviceProvider, apiKey);
            }
            else
            {
                throw new ArgumentException("Invalid mode", nameof(mode));
            }
        }
    }
}