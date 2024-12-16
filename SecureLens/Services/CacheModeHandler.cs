using SecureLens.Data;
using SecureLens.Logging;
using SecureLens.Models;
using SecureLens.Utilities;
using Microsoft.Extensions.Configuration;

namespace SecureLens.Services
{
    public class CacheModeHandler : IModeHandler
    {
        private readonly ILogger _logger;
        private readonly List<AdminByRequestSetting> _settings;
        private readonly IActiveDirectoryRepository _adRepo;
        private readonly IAdminByRequestRepository _abrRepo;
        private readonly string _reportPath;

        public CacheModeHandler(ILogger logger, List<AdminByRequestSetting> settings, IConfiguration configuration)
        {
            _logger = logger;
            _settings = settings;

            // Hent cache stier fra appsettings.json
            var cachePaths = configuration.GetSection("CachePaths");
            string groupCachePath = cachePaths.GetValue<string>("GroupCache");
            string userCachePath = cachePaths.GetValue<string>("UserCache");
            string cachedInventoryPath = cachePaths.GetValue<string>("Inventory");
            string cachedAuditLogsPath = cachePaths.GetValue<string>("AuditLogs");

            // Hent rapportstien fra appsettings.json
            _reportPath = configuration.GetValue<string>("ReportPath");

            // Opret ActiveDirectoryRepository med cache-strategi
            _adRepo = RepositoryFactory.CreateActiveDirectoryRepository(
                _logger,
                useLiveData: false,
                groupCacheFilePath: groupCachePath,
                userCacheFilePath: userCachePath
            );

            // Opret AdminByRequestRepository med cache-strategi
            string apiKey = ""; // Ingen API key i cache mode
            _abrRepo = RepositoryFactory.CreateAdminByRequestRepository(
                apiKey,
                _logger,
                useLiveData: false,
                cachedInventoryPath: cachedInventoryPath,
                cachedAuditLogsPath: cachedAuditLogsPath
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

                // Combine allesammen i CompletedUser liste
                _logger.LogInfo("Building Completed Users to prepare analysis...");
                var dataHandler = new DataHandler(
                    _abrRepo.LoadCachedAuditLogs("../../../../MockData/cached_auditlogs.json"),
                    _abrRepo.LoadCachedInventoryData("../../../../MockData/cached_auditlogs.json"),
                    _adRepo,
                    _logger
                );
                List<CompletedUser> completedUsers = dataHandler.BuildCompletedUsers();

                _logger.LogInfo($"[CACHE] Built {completedUsers.Count} CompletedUsers from the data.");

                // Initialize Analyzer med CompletedUsers og Settings
                var analyzer = new Analyzer(completedUsers, _settings);

                // Compute alle stats
                var overallStats = analyzer.ComputeOverallStatistics();
                var unusedGroups = analyzer.ComputeUnusedAdGroups(_adRepo); // Brug AD Repository
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
                File.WriteAllText(_reportPath, htmlContent);
                _logger.LogInfo($"[INFO] HTML report successfully written to '{_reportPath}'.");
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
        }
    }
}
