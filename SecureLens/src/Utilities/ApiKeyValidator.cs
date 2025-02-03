using System.Text.RegularExpressions;

namespace SecureLens.Utilities;

public static class ApiKeyValidator
{
    // Define regex-pattern for API-key with case-insensitive settings
    private static readonly Regex ApiKeyPattern = new(
        @"^[A-Z0-9]{8}(-[A-Z0-9]{4}){3}-[A-Z0-9]{12}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Validates API-key format.
    /// </summary>
    /// <param name="apiKey">API-key string</param>
    /// <returns>True if valid - else false</returns>
    public static bool IsValid(string? apiKey)
    {
        return !string.IsNullOrEmpty(apiKey) && ApiKeyPattern.IsMatch(apiKey);
    }
}