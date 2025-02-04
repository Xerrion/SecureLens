using Newtonsoft.Json;
using SecureLens.Core.Models;
using SecureLens.Infrastructure.Interfaces;
using SecureLens.Infrastructure.Logging;
using SecureLens.Utilities;

namespace SecureLens.Infrastructure.Strategies
{
    public class AdminByRequestStrategy(ILogger logger) : BaseHttpStrategy, IAdminByRequestStrategy
    {
        public async Task<List<InventoryLogEntry>> FetchInventoryDataAsync(
            string inventoryUrl,
            Dictionary<string, string> headers)
        {
            try
            {
                List<InventoryLogEntry> inventoryData = await SendRequestAsync<List<InventoryLogEntry>>(
                    HttpMethod.Get, inventoryUrl, headers);

                DataSanitizer.SanitizeInventoryLogs(inventoryData);
                logger.LogInfo($"Fetched {inventoryData.Count} inventory records from API.");
                return inventoryData;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError($"Failed to fetch inventory data: {ex.Message}");
                return [];
            }
            catch (JsonException ex)
            {
                logger.LogError($"JSON decode error while fetching inventory: {ex.Message}");
                return [];
            }
        }

        public async Task<List<AuditLogEntry>> FetchAuditLogsAsync(
            string auditUrl,
            Dictionary<string, string> headers,
            Dictionary<string, string> parameters)
        {
            var url = $"{auditUrl}?{BuildQueryString(parameters)}";
            try
            {
                List<AuditLogEntry>? auditLogs = await SendRequestAsync<List<AuditLogEntry>>(
                    HttpMethod.Get, url, headers);

                if (auditLogs != null)
                {
                    DataSanitizer.SanitizeAuditLogs(auditLogs);
                    logger.LogInfo($"Fetched {auditLogs.Count} audit log records from API.");
                    return auditLogs;
                }

                logger.LogError("Failed to decode audit logs from API.");
                return [];
            }
            catch (HttpRequestException ex)
            {
                logger.LogError($"Failed to fetch audit logs: {ex.Message}");
                return [];
            }
            catch (JsonException ex)
            {
                logger.LogError($"JSON decode error while fetching audit logs: {ex.Message}");
                return [];
            }
        }
    }
}