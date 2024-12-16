using SecureLens.Data;
using SecureLens.Logging;
using SecureLens.Models;
using Microsoft.Extensions.Configuration;
using SecureLens.Analysis.Results;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace SecureLens.Services
{
    public class OnlineModeHandler : IModeHandler
    {
        private readonly ILogger _logger;
        private readonly List<AdminByRequestSetting> _settings;
        private readonly string _apiKey;
        private readonly string _reportPath;
        private readonly string _groupCachePath;
        private readonly string _userCachePath;
        private readonly IConfiguration _configuration;
        private readonly Analyzer _analyzer; // Injected Analyzer

        public OnlineModeHandler(
            ILogger logger,
            List<AdminByRequestSetting> settings,
            string apiKey,
            IConfiguration configuration,
            Analyzer analyzer) // Injektionsparameter
        {
            _logger = logger;
            _settings = settings;
            _apiKey = apiKey;
            _configuration = configuration;
            _analyzer = analyzer;
            _reportPath = configuration.GetValue<string>("ReportPath");
            _groupCachePath = configuration.GetValue<string>("CachePaths:GroupCache");
            _userCachePath = configuration.GetValue<string>("CachePaths:UserCache");
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var abrRepo = RepositoryFactory.CreateAdminByRequestRepository(
                    _apiKey,
                    _logger,
                    useLiveData: true
                );

                var adRepo = RepositoryFactory.CreateActiveDirectoryRepository(
                    _logger,
                    useLiveData: true
                );

                var abrService = new AdminByRequestService(abrRepo);

                foreach (var setting in _settings)
                {
                    abrService.CreateSetting(setting.Name, setting.ActiveDirectoryGroups);
                    _logger.LogInfo($"Created setting: {setting.Name} containing {setting.ActiveDirectoryGroups.Count} AD-groups");
                }

                // Fetch Inventory Data (online)
                _logger.LogInfo("=== Inventory Data ===");
                List<InventoryLogEntry> inventory = await abrRepo.FetchInventoryDataAsync();
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
                List<AuditLogEntry> auditLogs = await abrRepo.FetchAuditLogsAsync(auditParams);
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
                var dataHandler = new DataHandler(auditLogs, inventory, adRepo, _logger);
                List<CompletedUser> completedUsers = dataHandler.BuildCompletedUsers();

                _logger.LogInfo($"[ONLINE] Built {completedUsers.Count} CompletedUsers from the data.");

                // Beregn alle stats ved hjælp af den injicerede Analyzer
                var overallStats = _analyzer.ComputeOverallStatistics(completedUsers, _settings);
                var unusedGroups = _analyzer.ComputeUnusedAdGroups(completedUsers, _settings, adRepo);
                var appStats = _analyzer.ComputeApplicationStatistics(completedUsers, _settings);
                List<TerminalStatisticsRow> terminalStats = _analyzer.ComputeTerminalStatistics(completedUsers, _settings);

                // Generér HTML rapport
                var htmlWriter = new HtmlReportWriter();
                string htmlContent = htmlWriter.BuildHtmlReport(
                    overallStats,
                    appStats,
                    terminalStats,
                    unusedGroups,
                    _settings
                );

                // Skriv til lokal fil
                File.WriteAllText(_reportPath, htmlContent);
                _logger.LogInfo($"[INFO] HTML report successfully written to '{_reportPath}'.");
            }
            catch (ArgumentNullException ex)
            {
                // Håndter specifikke undtagelser fra Analyzer eller andre klasser
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
                // Dette kræver, at _apiKey er et char array
                // Overvej at ændre _apiKey til en char array hvis nødvendigt
            }
        }
    }
}
