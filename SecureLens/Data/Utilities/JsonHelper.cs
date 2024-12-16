using Newtonsoft.Json;
using SecureLens.Logging;

namespace SecureLens.Utilities
{
    public static class JsonHelper
    {
        public static T? LoadJsonFile<T>(string filePath, ILogger? logger = null)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    logger?.LogError($"File not found: {filePath}");
                    return default;
                }
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                logger?.LogError($"Error deserializing JSON from {filePath}: {ex.Message}");
                return default;
            }
        }
    }
}