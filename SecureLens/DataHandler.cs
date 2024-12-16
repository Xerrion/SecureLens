using SecureLens.Data;
using SecureLens.Logging;
using SecureLens.Models;

namespace SecureLens
{
    /// <summary>
    /// DataHandler merges all data sources (AuditLogs, InventoryLogs, and AD data) 
    /// into a list of CompletedUser objects ready for further analysis.
    /// </summary>
    public class DataHandler
    {
        private readonly List<AuditLogEntry> _auditLogs;
        private readonly List<InventoryLogEntry> _inventoryLogs;
        private readonly IActiveDirectoryRepository _adRepo;
        private readonly ILogger _logger;

        // Dictionary of CompletedUser objects keyed by normalized user account
        private readonly Dictionary<string, CompletedUser> _completedUsers;

        public DataHandler(
            List<AuditLogEntry> auditLogs,
            List<InventoryLogEntry> inventoryLogs,
            IActiveDirectoryRepository adRepo,
            ILogger logger)
        {
            _auditLogs = auditLogs ?? new List<AuditLogEntry>();
            _inventoryLogs = inventoryLogs ?? new List<InventoryLogEntry>();
            _adRepo = adRepo;
            _logger = logger;
            _completedUsers = new Dictionary<string, CompletedUser>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Builds a list of CompletedUser objects by correlating AuditLogEntries, 
        /// InventoryLogEntries, and (optionally) AD user data.
        /// </summary>
        /// <returns>A list of CompletedUser objects.</returns>
        public List<CompletedUser> BuildCompletedUsers()
        {
            // 1) Process Audit Logs
            foreach (var auditEntry in _auditLogs)
            {
                string rawAccount = auditEntry.User?.Account;
                if (string.IsNullOrEmpty(rawAccount)) continue;

                string normalizedAccount = NormalizeUserAccount(rawAccount);
                if (!_completedUsers.ContainsKey(normalizedAccount))
                {
                    _completedUsers[normalizedAccount] = new CompletedUser
                    {
                        AccountName = normalizedAccount
                    };
                }
                _completedUsers[normalizedAccount].AddAuditLogEntry(auditEntry);
            }

            // 2) Process Inventory Logs
            foreach (var invEntry in _inventoryLogs)
            {
                string rawAccount = invEntry.User?.Account;
                if (string.IsNullOrEmpty(rawAccount)) continue;

                string normalizedAccount = NormalizeUserAccount(rawAccount);
                if (!_completedUsers.ContainsKey(normalizedAccount))
                {
                    _completedUsers[normalizedAccount] = new CompletedUser
                    {
                        AccountName = normalizedAccount
                    };
                }
                _completedUsers[normalizedAccount].AddInventoryLogEntry(invEntry);
            }

            // 3) Attach AD data if _adRepo is available
            if (_adRepo != null)
            {
                foreach (var kvp in _completedUsers)
                {
                    string normalizedAccount = kvp.Key;  // e.g. "user0192"
                    ActiveDirectoryUser adUser = _adRepo.GetAdUserFromFile(normalizedAccount);
                    if (adUser != null)
                    {
                        kvp.Value.ActiveDirectoryUser = adUser;
                    }
                }
            }
            return _completedUsers.Values.ToList();
        }

        /// <summary>
        /// Strips any "DOMAIN\" prefix from the user account for consistency
        /// </summary>
        private string NormalizeUserAccount(string rawAccount)
        {
            if (string.IsNullOrEmpty(rawAccount)) return rawAccount;

            // If the account contains a backslash, remove everything up to (and including) the backslash.
            // e.g. "DOMAIN\\user0192" => "user0192"
            int slashIndex = rawAccount.IndexOf('\\');
            if (slashIndex >= 0 && slashIndex < rawAccount.Length - 1)
            {
                rawAccount = rawAccount.Substring(slashIndex + 1);
            }
            return rawAccount.Trim();
        }
    }
}
