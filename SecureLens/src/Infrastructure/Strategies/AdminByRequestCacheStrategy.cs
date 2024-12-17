using SecureLens.Core.Models;
using SecureLens.Infrastructure.Interfaces;
using SecureLens.Infrastructure.Logging;
using SecureLens.Infrastructure.Utilities;
using SecureLens.Utilities;

namespace SecureLens.Infrastructure.Strategies;

public class AdminByRequestCacheStrategy : IAdminByRequestCacheStrategy
{
    private List<InventoryLogEntry> _inventoryCache = new();
    private List<AuditLogEntry> _auditLogsCache = new();
    private readonly ILogger _logger;

    public AdminByRequestCacheStrategy(ILogger logger)
    {
        _logger = logger;
    }

    public List<InventoryLogEntry> LoadCachedInventoryData(string filePath)
    {
        List<InventoryLogEntry>? data = JsonHelper.LoadJsonFile<List<InventoryLogEntry>>(filePath);
        if (data == null)
        {
            _logger.LogError($"Failed to load cached inventory data from {filePath}.");
            return new List<InventoryLogEntry>();
        }

        _inventoryCache = data;
        _logger.LogInfo($"Loaded {data.Count} cached inventory records from {filePath}.");
        return data;
    }

    public List<AuditLogEntry> LoadCachedAuditLogs(string filePath)
    {
        List<AuditLogEntry>? data = JsonHelper.LoadJsonFile<List<AuditLogEntry>>(filePath);
        if (data == null)
        {
            _logger.LogError($"Failed to load cached audit logs from {filePath}.");
            return new List<AuditLogEntry>();
        }

        _auditLogsCache = data;
        _logger.LogInfo($"Loaded {data.Count} cached audit log records from {filePath}.");
        return data;
    }

    public Task<List<InventoryLogEntry>> FetchInventoryDataAsync(string inventoryUrl,
        Dictionary<string, string> headers)
    {
        _logger.LogWarning("AdminByRequestCacheStrategy: FetchInventoryDataAsync is not supported in cached mode.");
        return Task.FromResult(new List<InventoryLogEntry>());
    }

    public Task<List<AuditLogEntry>> FetchAuditLogsAsync(string auditUrl, Dictionary<string, string> headers,
        Dictionary<string, string> parameters)
    {
        _logger.LogWarning("AdminByRequestCacheStrategy: FetchAuditLogsAsync is not supported in cached mode.");
        return Task.FromResult(new List<AuditLogEntry>());
    }
}