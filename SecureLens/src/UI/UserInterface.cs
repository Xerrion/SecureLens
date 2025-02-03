// UserInterface.cs

using Microsoft.Extensions.Configuration;
using SecureLens.Utilities;

namespace SecureLens.UI;

public class UserInterface(IConfiguration configuration)
{
    public string GetMode()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Choose 'cache' or 'online': ");
        Console.ResetColor();

        var mode = Console.ReadLine()?.Trim().ToLower();
        return mode ?? "cache"; // Standard til 'cache' hvis null
    }

    public string? GetApiKey()
    {
        var apiKey = configuration.GetSection("ApiKey").Get<string>();
        var isValid = ApiKeyValidator.IsValid(apiKey);
        if (isValid) return apiKey;
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Invalid API key format. Please try again.");
        Console.ResetColor();
        
        return null;
    }
}