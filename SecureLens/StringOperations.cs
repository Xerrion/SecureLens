using System;

namespace SecureLens
{
    public static class StringOperations
    {
        public static bool ContainsSecure(string input)
        {
            return input.Contains("secure", StringComparison.OrdinalIgnoreCase);
        }
    }
}