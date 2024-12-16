using System.Net.Http.Headers;
using Newtonsoft.Json;
using SecureLens.Logging;

namespace SecureLens.Data;

public class AdminByRequestRepository : IAdminByRequestRepository
{
    private static readonly HttpClient Client = new HttpClient();
    private readonly Dictionary<string, string> Headers;
    private readonly Dictionary<string, string> QueryParamsForInventory;
    private readonly ILogger _logger;

    private readonly string Take;
    private readonly string WantGroups;
    private readonly string Status;
    private readonly string StartDate;
    private readonly string EndDate;

    public string ApiKey { get; }
    public string BaseUrlInventory { get; }
    public string InventoryUrl { get; set; }
    public string BaseUrlAudit { get; }

    public AdminByRequestRepository(string apiKey, ILogger logger)
    {
        ApiKey = apiKey;
        _logger = logger;
        BaseUrlAudit = "https://dc1api.adminbyrequest.com/auditlog";
        Take = "1000";
        WantGroups = "1";
        QueryParamsForInventory = new Dictionary<string, string>
        {
            { "take", Take },
            { "wantgroups", WantGroups }
        };
        InventoryUrl = $"{BaseUrlInventory}?{BuildQueryString(QueryParamsForInventory)}";
        Status = "Finished";
        StartDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
        EndDate = DateTime.Now.ToString("yyyy-MM-dd");
        Headers = new Dictionary<string, string>
        {
            { "apikey", ApiKey }
        };
    }

    public async Task<List<InventoryLogEntry>> FetchInventoryDataAsync()
    {
        try
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, InventoryUrl))
            {
                foreach (var header in Headers)
                    request.Headers.Add(header.Key, header.Value);

                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await Client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var inventoryData = JsonConvert.DeserializeObject<List<InventoryLogEntry>>(content);
                        DataSanitizer.SanitizeInventoryLogs(inventoryData);
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
                    return new List<InventoryLogEntry>();
                }
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

    public async Task<List<AuditLogEntry>> FetchAuditLogsAsync(Dictionary<string, string> @params)
    {
        var url = $"{BaseUrlAudit}?{BuildQueryString(@params)}";

        try
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                foreach (var header in Headers)
                    request.Headers.Add(header.Key, header.Value);

                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await Client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var auditLogs = JsonConvert.DeserializeObject<List<AuditLogEntry>>(content);
                        DataSanitizer.SanitizeAuditLogs(auditLogs);
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

    public List<InventoryLogEntry> LoadCachedInventoryData(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogError($"File not found: {filePath}");
            return new List<InventoryLogEntry>();
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var inventoryData = JsonConvert.DeserializeObject<List<InventoryLogEntry>>(json);

            DataSanitizer.SanitizeInventoryLogs(inventoryData);

            return inventoryData;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"JSON parse error for cached inventory: {ex.Message}");
            return new List<InventoryLogEntry>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading cached inventory file: {ex.Message}");
            return new List<InventoryLogEntry>();
        }
    }

    public List<AuditLogEntry> LoadCachedAuditLogs(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogError($"File not found: {filePath}");
            return new List<AuditLogEntry>();
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var auditLogs = JsonConvert.DeserializeObject<List<AuditLogEntry>>(json);

            DataSanitizer.SanitizeAuditLogs(auditLogs);
            return auditLogs;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"JSON parse error for cached audit logs: {ex.Message}");
            return new List<AuditLogEntry>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading cached audit log file: {ex.Message}");
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