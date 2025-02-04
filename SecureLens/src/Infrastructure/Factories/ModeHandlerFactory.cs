using Microsoft.Extensions.DependencyInjection;
using SecureLens.Application.Services;
using SecureLens.Application.Services.Interfaces;

namespace SecureLens.Infrastructure.Factories;

public class ModeHandlerFactory(IServiceProvider serviceProvider)
{
    public IModeHandler CreateModeHandler(string mode, string apiKey = "")
    {
        var apiKeyChars = apiKey.ToCharArray();

        return mode.ToLower() switch 
        {
            "cache" => serviceProvider.GetRequiredService<CacheModeHandler>(),
            "online" => ActivatorUtilities.CreateInstance<OnlineModeHandler>(serviceProvider, apiKeyChars),
            _ => throw new ArgumentException("Invalid mode", nameof(mode))
        };
    }
}