using System;
using System.Text.RegularExpressions;

namespace DefaultNamespace
{
    public static class ApiKeyValidator
    {
        // Define the regex pattern for the API key
        private static readonly Regex ApiKeyPattern = new Regex(
            @"^[A-Z0-9]{8}(-[A-Z0-9]{4}){3}-[A-Z0-9]{12}$",
            RegexOptions.Compiled);

        /// <summary>
        /// Validates the API key format.
        /// </summary>
        /// <param name="apiKey">The API key string to validate.</param>
        /// <returns>True if valid; otherwise, false.</returns>
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