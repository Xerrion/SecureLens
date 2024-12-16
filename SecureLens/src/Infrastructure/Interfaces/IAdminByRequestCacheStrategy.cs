using SecureLens.Core.Models;

namespace SecureLens.Infrastructure.Interfaces
{
    public interface IAdminByRequestCacheStrategy : IAdminByRequestStrategy
    {
        List<InventoryLogEntry> LoadCachedInventoryData(string filePath);
        List<AuditLogEntry> LoadCachedAuditLogs(string filePath);
    }
}