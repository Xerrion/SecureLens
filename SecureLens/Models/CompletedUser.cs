namespace SecureLens;

public class CompletedUser
{
    public string AccountName { get; set; }
    public List<AuditLogEntry> AuditLogEntries { get; set; }
    public List<InventoryLogEntry> InventoryLogEntries { get; set; }
    public ActiveDirectoryUser? ActiveDirectoryUser { get; set; }
    
    public CompletedUser()
    {
        AuditLogEntries = new List<AuditLogEntry>();
        InventoryLogEntries = new List<InventoryLogEntry>();
    }
    
    public void AddAuditLogEntry(AuditLogEntry entry)
    {
        AuditLogEntries.Add(entry);
    }
    
    public void AddInventoryLogEntry(InventoryLogEntry entry)
    {
        InventoryLogEntries.Add(entry);
    }
}