using SecureLens.Core.Models;

namespace SecureLens.Infrastructure.Interfaces;

public interface IAdminByRequestRepository
{
    public Task<List<InventoryLogEntry>> FetchInventoryDataAsync();
    public Task<List<AuditLogEntry>> FetchAuditLogsAsync(Dictionary<string, string> @params);

    public List<InventoryLogEntry> LoadCachedInventoryData(string filePath);
    public List<AuditLogEntry> LoadCachedAuditLogs(string filePath);
    public string ApiKey { get; }
    public string BaseUrlInventory { get; }
    public string BaseUrlAudit { get; }
}