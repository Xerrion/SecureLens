namespace SecureLens.Data.Strategies.Interfaces
{
    public interface IAdminByRequestCacheStrategy : IAdminByRequestStrategy
    {
        List<InventoryLogEntry> LoadCachedInventoryData(string filePath);
        List<AuditLogEntry> LoadCachedAuditLogs(string filePath);
    }
}