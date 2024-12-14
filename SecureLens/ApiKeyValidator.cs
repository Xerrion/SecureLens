using System;
using System.Text.RegularExpressions;

namespace DefaultNamespace
{
    public static class ApiKeyValidator
    {
        // Definer regex-mønstret for API-nøglen med case-insensitive indstillinger
        private static readonly Regex ApiKeyPattern = new Regex(
            @"^[A-Z0-9]{8}(-[A-Z0-9]{4}){3}-[A-Z0-9]{12}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Validerer API-nøgleformatet.
        /// </summary>
        /// <param name="apiKey">API-nøglestrengen, der skal valideres.</param>
        /// <returns>True hvis gyldig; ellers, false.</returns>
        public static bool IsValid(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return false;
            }

            return ApiKeyPattern.IsMatch(apiKey);
        }
    }
}