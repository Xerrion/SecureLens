namespace SecureLens;

public interface IAdminByRequestRepository
{
    Task<List<InventoryLogEntry>> FetchInventoryDataAsync();
    Task<List<AuditLogEntry>> FetchAuditLogsAsync(Dictionary<string, string> @params);

    List<InventoryLogEntry> LoadCachedInventoryData(string filePath);
    List<AuditLogEntry> LoadCachedAuditLogs(string filePath);
    string ApiKey { get; }
    string BaseUrlInventory { get; }
    string BaseUrlAudit { get; }
}