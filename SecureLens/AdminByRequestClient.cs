using SecureLens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DefaultNamespace
{
    public class AdminByRequestClient
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string BaseUrlAudit;
        private readonly string BaseUrlInventory;
        private readonly string ApiKey;
        private readonly Dictionary<string, string> Headers;
        private readonly string StartDate;
        private readonly string EndDate;
        private readonly string Status;
        private readonly string Take;
        private readonly string WantsScanDetails;
        private readonly string WantGroups;
        private readonly string Type;

        public AdminByRequestClient(string apiKey)
        {
            BaseUrlInventory = "https://dc1api.adminbyrequest.com/inventory";
            BaseUrlAudit = "https://dc1api.adminbyrequest.com/auditlog";
            ApiKey = apiKey;
            StartDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            Status = "Finished";
            Take = "100"; 
            WantGroups = "1"; 
            Headers = new Dictionary<string, string>
            {
                { "apikey", ApiKey }
            };
        }

        /// <summary>
        /// Fetches inventory data asynchronously from Admin By Request.
        /// </summary>
        /// <returns>A list of InventoryLogEntry objects. Returns an empty list if the fetch fails.</returns>
        public async Task<List<InventoryLogEntry>> FetchInventoryDataAsync()
        {
            // Set console color to Cyan for fetching message
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Fetching inventory data from Admin By Request...");
            Console.ResetColor();

            // Define query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "take", Take },
                { "wantgroups", WantGroups }
                // Add other query parameters as needed
            };

            // Build the full URL with query parameters
            var url = $"{BaseUrlInventory}?{BuildQueryString(queryParams)}";

            try
            {
                // Create the HTTP request
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    // Add headers to the request
                    foreach (var header in Headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    // Add the Accept header to indicate that we expect JSON response
                    request.Headers.Accept.Clear();
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Send the request asynchronously
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        try
                        {
                            // Deserialize JSON response to a list of InventoryLogEntry
                            var inventoryData = JsonConvert.DeserializeObject<List<InventoryLogEntry>>(content);

                            // Sanitize the fetched data
                            DataSanitizer.SanitizeInventoryLogs(inventoryData);

                            // Set console color to Green for success message
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Fetched {inventoryData.Count} inventory records.");
                            Console.ResetColor();

                            return inventoryData;
                        }
                        catch (JsonException e)
                        {
                            // Set console color to Red for JSON errors
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"JSON decode error while fetching inventory: {e.Message}");
                            Console.ResetColor();
                            return new List<InventoryLogEntry>();
                        }
                    }
                    else
                    {
                        // Set console color to Red for HTTP errors
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(
                            $"Failed to fetch inventory data. Status Code: {(int)response.StatusCode} ({response.ReasonPhrase})");
                        Console.ResetColor();
                        return new List<InventoryLogEntry>();
                    }
                }
            }
            catch (HttpRequestException e)
            {
                // Handle network-related errors
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Request error while fetching inventory data: {e.Message}");
                Console.ResetColor();
                return new List<InventoryLogEntry>();
            }
            catch (Exception e)
            {
                // Handle all other exceptions
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An unexpected error occurred: {e.Message}");
                Console.ResetColor();
                return new List<InventoryLogEntry>();
            }
        }

        /// <summary>
        /// Fetches audit log entries asynchronously from Admin By Request without pagination.
        /// </summary>
        /// <param name="params">A dictionary of query parameters to filter audit logs.</param>
        /// <returns>A list of AuditLogEntry objects. Returns an empty list if the fetch fails.</returns>
        public async Task<List<AuditLogEntry>> FetchAuditLogsAsync(Dictionary<string, string> @params)
        {
            // Set console color to Cyan for fetching message
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Fetching audit logs from Admin By Request...");
            Console.ResetColor();

            // Build the full URL with query parameters
            var url = $"{BaseUrlAudit}?{BuildQueryString(@params)}";

            try
            {
                // Create the HTTP request
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    // Add headers to the request
                    foreach (var header in Headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    // Add the Accept header to indicate that we expect JSON response
                    request.Headers.Accept.Clear();
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Send the request asynchronously
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        try
                        {
                            // Deserialize JSON response to a list of AuditLogEntry
                            var auditLogs = JsonConvert.DeserializeObject<List<AuditLogEntry>>(content);

                            // Sanitize the fetched data
                            DataSanitizer.SanitizeAuditLogs(auditLogs);

                            // Set console color to Green for success message
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Fetched {auditLogs.Count} audit log records.");
                            Console.ResetColor();

                            return auditLogs;
                        }
                        catch (JsonException e)
                        {
                            // Set console color to Red for JSON errors
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"JSON decode error while fetching audit logs: {e.Message}");
                            Console.ResetColor();
                            return new List<AuditLogEntry>();
                        }
                    }
                    else
                    {
                        // Set console color to Red for HTTP errors
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(
                            $"Failed to fetch audit logs. Status Code: {(int)response.StatusCode} ({response.ReasonPhrase})");
                        Console.ResetColor();

                        // Optionally, log the response body for more details
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Response: {errorContent}");
                        Console.ResetColor();

                        return new List<AuditLogEntry>();
                    }
                }
            }
            catch (HttpRequestException e)
            {
                // Handle network-related errors
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Request error while fetching audit logs: {e.Message}");
                Console.ResetColor();
                return new List<AuditLogEntry>();
            }
            catch (Exception e)
            {
                // Handle all other exceptions
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An unexpected error occurred: {e.Message}");
                Console.ResetColor();
                return new List<AuditLogEntry>();
            }
        }

        /// <summary>
        /// Builds a query string from a dictionary of parameters.
        /// </summary>
        /// <param name="parameters">Dictionary of query parameters.</param>
        /// <returns>A URL-encoded query string.</returns>
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
