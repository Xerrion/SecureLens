using SecureLens.Data.Strategies.Interfaces;
using SecureLens.Logging;
using SecureLens.Models;
using SecureLens.Utilities;

namespace SecureLens.Data
{
    public class AdminByRequestRepository : BaseRepository, IAdminByRequestRepository
    {
        private readonly IAdminByRequestStrategy _strategy;
        private readonly string _baseUrlInventory;
        private readonly string _baseUrlAudit;

        public string ApiKey { get; }
        public string BaseUrlInventory => _baseUrlInventory;
        public string BaseUrlAudit => _baseUrlAudit;

        public AdminByRequestRepository(string apiKey, ILogger logger, IAdminByRequestStrategy strategy)
            : base(logger)
        {
            ApiKey = apiKey;
            _strategy = strategy;
            _baseUrlInventory = "https://dc1api.adminbyrequest.com/inventory";
            _baseUrlAudit = "https://dc1api.adminbyrequest.com/auditlog";
        }

        public async Task<List<InventoryLogEntry>> FetchInventoryDataAsync()
        {
            var headers = new Dictionary<string, string>
            {
                { "apikey", ApiKey }
            };

            return await _strategy.FetchInventoryDataAsync(_baseUrlInventory, headers);
        }

        public async Task<List<AuditLogEntry>> FetchAuditLogsAsync(Dictionary<string, string> @params)
        {
            var headers = new Dictionary<string, string>
            {
                { "apikey", ApiKey }
            };

            return await _strategy.FetchAuditLogsAsync(_baseUrlAudit, headers, @params);
        }

        public List<InventoryLogEntry> LoadCachedInventoryData(string filePath)
        {
            if (_strategy is IAdminByRequestCacheStrategy cachedStrategy)
            {
                return cachedStrategy.LoadCachedInventoryData(filePath);
            }
            else
            {
                Logger.LogWarning("LoadCachedInventoryData called on non-cached strategy.");
                return new List<InventoryLogEntry>();
            }
        }

        public List<AuditLogEntry> LoadCachedAuditLogs(string filePath)
        {
            if (_strategy is IAdminByRequestCacheStrategy cachedStrategy)
            {
                return cachedStrategy.LoadCachedAuditLogs(filePath);
            }
            else
            {
                Logger.LogWarning("LoadCachedAuditLogs called on non-cached strategy.");
                return new List<AuditLogEntry>();
            }
        }
    }
}
