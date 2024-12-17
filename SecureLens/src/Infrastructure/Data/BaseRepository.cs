using System.Text.Json;
using SecureLens.Infrastructure.Logging;

namespace SecureLens.Infrastructure.Data;

public abstract class BaseRepository
{
    protected readonly ILogger Logger;

    protected BaseRepository(ILogger logger)
    {
        Logger = logger;
    }

    protected void LogError(Exception ex, string contextMessage)
    {
        Logger.LogError($"{contextMessage}: {ex.Message}");
    }

    // Evt. fælles File-check/logik:
    protected bool FileExistsOrWarn(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Logger.LogError($"File not found: {filePath}");
            return false;
        }

        return true;
    }

    // Evt. fælles logik til JSON-læsning:
    protected T? LoadJsonFile<T>(string filePath)
    {
        try
        {
            if (!FileExistsOrWarn(filePath)) return default;
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            LogError(ex, $"Error loading JSON from {filePath}");
            return default;
        }
    }
}