// OnlineModeHandler.cs
using SecureLens.Data;
using SecureLens.Logging;
using SecureLens.Models;
using SecureLens.Utilities;

namespace SecureLens.Services
{
    public class OnlineModeHandler : IModeHandler
    {
        private readonly ILogger _logger;
        private readonly List<AdminByRequestSetting> _settings;
        private readonly IActiveDirectoryRepository _adRepo;
        private readonly IAdminByRequestRepository _abrRepo;
        private readonly string _apiKey;

        public OnlineModeHandler(ILogger logger, List<AdminByRequestSetting> settings, string apiKey)
        {
            _logger = logger;
            _settings = settings;
            _apiKey = apiKey;

            // Opret ActiveDirectoryRepository med live-strategi
            _adRepo = RepositoryFactory.CreateActiveDirectoryRepository(
                _logger,
                useLiveData: true
            );

            // Opret AdminByRequestRepository med live-strategi
            _abrRepo = RepositoryFactory.CreateAdminByRequestRepository(
                _apiKey,
                _logger,
                useLiveData: true
            );
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var abrService = new AdminByRequestService(_abrRepo);

                foreach (var setting in _settings)
                {
                    abrService.CreateSetting(setting.Name, setting.ActiveDirectoryGroups);
                    _logger.LogInfo($"Created setting: {setting.Name} containing {setting.ActiveDirectoryGroups.Count} AD-groups");
                }

                // Fetch Inventory Data (online)
                _logger.LogInfo("=== Inventory Data ===");
                List<InventoryLogEntry> inventory = await _abrRepo.FetchInventoryDataAsync();
                if (inventory.Count > 0)
                {
                    _logger.LogInfo($"Fetched {inventory.Count} inventory logs (online).");
                }
                else
                {
                    _logger.LogInfo("No inventory data fetched (online).");
                }

                // Fetch Audit Logs (online)
                _logger.LogInfo("=== Audit Logs ===");
                Dictionary<string, string> auditParams = new Dictionary<string, string>
                {
                    { "take", "1000" }, // Juster efter behov
                    { "wantscandetails", "1" },
                    { "startdate", "2023-01-01" },
                    { "enddate", "2025-12-31" },
                    { "status", "Finished" },
                    { "type", "app" }
                };
                List<AuditLogEntry> auditLogs = await _abrRepo.FetchAuditLogsAsync(auditParams);
                if (auditLogs.Count > 0)
                {
                    _logger.LogInfo($"Fetched {auditLogs.Count} audit logs (online).");
                }
                else
                {
                    _logger.LogInfo("No audit log data fetched (online).");
                }

                // Combine allesammen i CompletedUser liste
                _logger.LogInfo("Building Completed Users to prepare analysis...");
                var dataHandler = new DataHandler(auditLogs, inventory, _adRepo, _logger);
                List<CompletedUser> completedUsers = dataHandler.BuildCompletedUsers();

                _logger.LogInfo($"[ONLINE] Built {completedUsers.Count} CompletedUsers from the data.");

                // Initialize Analyzer med CompletedUsers og Settings
                var analyzer = new Analyzer(completedUsers, _settings);

                // Compute alle stats
                var overallStats = analyzer.ComputeOverallStatistics();
                var unusedGroups = analyzer.ComputeUnusedAdGroups(_adRepo);
                var appStats = analyzer.ComputeApplicationStatistics();
                List<Analyzer.TerminalStatisticsRow> terminalStats = analyzer.ComputeTerminalStatistics();

                // Generate HTML report
                var htmlWriter = new HtmlReportWriter();
                string htmlContent = htmlWriter.BuildHtmlReport(
                    overallStats,
                    appStats,
                    terminalStats,
                    unusedGroups,
                    _settings
                );

                // Write to local file
                string outputHtmlFile = @"C:\Users\jeppe\Desktop\report.html";
                File.WriteAllText(outputHtmlFile, htmlContent);
                _logger.LogInfo($"[INFO] HTML report successfully written to '{outputHtmlFile}'.");
            }
            catch (ArgumentNullException ex)
            {
                // Handle specifikke undtagelser fra Analyzer eller andre klasser
                _logger.LogError($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Håndter uventede undtagelser
                _logger.LogError($"An unexpected error occurred: {ex.Message}");
            }
            finally
            {
                // Overwrite the API key in memory for security
                for (int i = 0; i < _apiKey.Length; i++)
                {
                    // Dette kræver, at _apiKey er et char array
                    // Overvej at ændre _apiKey til en char array hvis nødvendigt
                }
            }
        }
    }
}
