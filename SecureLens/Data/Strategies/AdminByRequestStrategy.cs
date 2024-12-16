using SecureLens.Data.Strategies.Interfaces;
using SecureLens.Logging;
using SecureLens.Models;
using SecureLens.Utilities;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace SecureLens.Data.Strategies
{
    public class AdminByRequestStrategy : IAdminByRequestStrategy
    {
        private readonly ILogger _logger;
        private static readonly HttpClient _client = new HttpClient();

        public AdminByRequestStrategy(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<InventoryLogEntry>> FetchInventoryDataAsync(string inventoryUrl, Dictionary<string, string> headers)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, inventoryUrl);
                foreach (var header in headers)
                    request.Headers.Add(header.Key, header.Value);

                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var inventoryData = JsonConvert.DeserializeObject<List<InventoryLogEntry>>(content);
                        DataSanitizer.SanitizeInventoryLogs(inventoryData);
                        _logger.LogInfo($"Fetched {inventoryData.Count} inventory records from API.");
                        return inventoryData;
                    }
                    catch (JsonException e)
                    {
                        _logger.LogError($"JSON decode error while fetching inventory: {e.Message}");
                        return new List<InventoryLogEntry>();
                    }
                }
                else
                {
                    _logger.LogError($"Failed to fetch inventory data. Status Code: {(int)response.StatusCode} ({response.ReasonPhrase})");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Response: {errorContent}");
                    return new List<InventoryLogEntry>();
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Request error while fetching inventory data: {e.Message}");
                return new List<InventoryLogEntry>();
            }
            catch (Exception e)
            {
                _logger.LogError($"An unexpected error occurred: {e.Message}");
                return new List<InventoryLogEntry>();
            }
        }

        public async Task<List<AuditLogEntry>> FetchAuditLogsAsync(string auditUrl, Dictionary<string, string> headers, Dictionary<string, string> parameters)
        {
            var url = $"{auditUrl}?{BuildQueryString(parameters)}";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                foreach (var header in headers)
                    request.Headers.Add(header.Key, header.Value);

                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var auditLogs = JsonConvert.DeserializeObject<List<AuditLogEntry>>(content);
                        DataSanitizer.SanitizeAuditLogs(auditLogs);
                        _logger.LogInfo($"Fetched {auditLogs.Count} audit log records from API.");
                        return auditLogs;
                    }
                    catch (JsonException e)
                    {
                        _logger.LogError($"JSON decode error while fetching audit logs: {e.Message}");
                        return new List<AuditLogEntry>();
                    }
                }
                else
                {
                    _logger.LogError($"Failed to fetch audit logs. Status Code: {(int)response.StatusCode} ({response.ReasonPhrase})");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Response: {errorContent}");
                    return new List<AuditLogEntry>();
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Request error while fetching audit logs: {e.Message}");
                return new List<AuditLogEntry>();
            }
            catch (Exception e)
            {
                _logger.LogError($"An unexpected error occurred: {e.Message}");
                return new List<AuditLogEntry>();
            }
        }

        private string BuildQueryString(Dictionary<string, string> parameters)
        {
            var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
            foreach (var param in parameters)
            {
                query[param.Key] = param.Value;
            }
            return query.ToString();
        }
    }
}
