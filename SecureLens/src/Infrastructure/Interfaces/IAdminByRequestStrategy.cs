using SecureLens.Core.Models;

namespace SecureLens.Infrastructure.Interfaces;

public interface IAdminByRequestStrategy
{
    public Task<List<InventoryLogEntry>> FetchInventoryDataAsync(string inventoryUrl,
        Dictionary<string, string> headers);

    public Task<List<AuditLogEntry>> FetchAuditLogsAsync(string auditUrl, Dictionary<string, string> headers,
        Dictionary<string, string> parameters);
}