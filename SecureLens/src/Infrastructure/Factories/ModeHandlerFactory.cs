using Microsoft.Extensions.DependencyInjection;
using SecureLens.Application.Services;
using SecureLens.Application.Services.Interfaces;

namespace SecureLens.Infrastructure.Factories;

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
            return _serviceProvider.GetRequiredService<CacheModeHandler>();
        else if (mode.Equals("online", StringComparison.OrdinalIgnoreCase))
        {
            // Convert the API key string to a char array
            char[] apiKeyChars = apiKey.ToCharArray();
            return ActivatorUtilities.CreateInstance<OnlineModeHandler>(_serviceProvider, apiKeyChars);
        }
        else
            throw new ArgumentException("Invalid mode", nameof(mode));
    }
}
