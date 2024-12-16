using Microsoft.Extensions.Configuration;
using SecureLens.Application.Analysis.Results;
using SecureLens.Application.Services.Interfaces;
using SecureLens.Core.Models;
using SecureLens.Infrastructure.Factories;
using SecureLens.Infrastructure.Logging;
using SecureLens.Utilities;

namespace SecureLens.Application.Services
{
    public class CacheModeHandler : IModeHandler
    {
        private readonly ILogger _logger;
        private readonly List<AdminByRequestSetting> _settings;
        private readonly string _reportPath;
        private readonly string _auditCachePath;
        private readonly string _inventoryCachePath;
        private readonly string _groupCachePath;
        private readonly string _userCachePath;
        private readonly IConfiguration _configuration;
        private readonly Analyzer _analyzer; // Injected Analyzer

        public CacheModeHandler(
            ILogger logger,
            List<AdminByRequestSetting> settings,
            IConfiguration configuration,
            Analyzer analyzer) // Injektionsparameter
        {
            _logger = logger;
            _settings = settings;
            _configuration = configuration;
            _analyzer = analyzer;
            _reportPath = configuration.GetValue<string>("ReportPath");
            _auditCachePath = configuration.GetValue<string>("CachePaths:AuditLogs");
            _inventoryCachePath = configuration.GetValue<string>("CachePaths:Inventory");
            _groupCachePath = configuration.GetValue<string>("CachePaths:GroupCache");
            _userCachePath = configuration.GetValue<string>("CachePaths:UserCache");
        }

        public async Task ExecuteAsync()
        {
            try
            {
                // Opret AdminByRequestRepository i cache mode
                string apiKey = ""; // Ingen API key i cache mode
                var abrRepo = RepositoryFactory.CreateAdminByRequestRepository(
                    apiKey,
                    _logger,
                    useLiveData: false,
                    cachedInventoryPath: _inventoryCachePath,
                    cachedAuditLogsPath: _auditCachePath
                );

                // Opret ActiveDirectoryRepository i cache mode
                var adRepo = RepositoryFactory.CreateActiveDirectoryRepository(
                    _logger,
                    useLiveData: false,
                    groupCacheFilePath: _groupCachePath,
                    userCacheFilePath: _userCachePath
                );

                var abrService = new AdminByRequestService(abrRepo);

                foreach (var setting in _settings)
                {
                    abrService.CreateSetting(setting.Name, setting.ActiveDirectoryGroups);
                    _logger.LogInfo($"Created setting: {setting.Name} containing {setting.ActiveDirectoryGroups.Count} AD-groups");
                }

                // Kombinér data i CompletedUser liste
                _logger.LogInfo("Building Completed Users to prepare analysis...");
                var dataHandler = new DataHandler(
                    abrRepo.LoadCachedAuditLogs(_auditCachePath),
                    abrRepo.LoadCachedInventoryData(_inventoryCachePath),
                    adRepo,
                    _logger
                );
                List<CompletedUser> completedUsers = dataHandler.BuildCompletedUsers();

                _logger.LogInfo($"[CACHE] Built {completedUsers.Count} CompletedUsers from the data.");

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
        }
    }
}
