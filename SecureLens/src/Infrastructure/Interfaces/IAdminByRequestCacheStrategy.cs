using SecureLens.Core.Models;

namespace SecureLens.Infrastructure.Interfaces;

public interface IAdminByRequestCacheStrategy : IAdminByRequestStrategy
{
    public List<InventoryLogEntry> LoadCachedInventoryData(string filePath);
    public List<AuditLogEntry> LoadCachedAuditLogs(string filePath);
}