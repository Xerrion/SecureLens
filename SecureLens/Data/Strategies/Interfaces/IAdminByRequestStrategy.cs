namespace SecureLens.Data.Strategies.Interfaces;

public interface IAdminByRequestStrategy
{
    Task<List<InventoryLogEntry>> FetchInventoryDataAsync(string inventoryUrl, Dictionary<string, string> headers);
    Task<List<AuditLogEntry>> FetchAuditLogsAsync(string auditUrl, Dictionary<string, string> headers, Dictionary<string, string> parameters);
}