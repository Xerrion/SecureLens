using Microsoft.Extensions.Configuration;
using SecureLens.Application.Analysis.Results;
using SecureLens.Application.Services.Interfaces;
using SecureLens.Core.Models;
using SecureLens.Infrastructure.Factories;
using SecureLens.Infrastructure.Interfaces;
using SecureLens.Infrastructure.Logging;
using SecureLens.Utilities;

namespace SecureLens.Application.Services;

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
        Analyzer analyzer) // Injection parameter
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
            // Create AdminByRequestRepository in cache mode
            var apiKey = ""; // No API key in cache mode
            IAdminByRequestRepository abrRepo = RepositoryFactory.CreateAdminByRequestRepository(
                apiKey,
                _logger,
                false,
                _inventoryCachePath,
                _auditCachePath
            );

            // Create ActiveDirectoryRepository in cache mode
            IActiveDirectoryRepository adRepo = RepositoryFactory.CreateActiveDirectoryRepository(
                _logger,
                false,
                _groupCachePath,
                _userCachePath
            );

            var abrService = new AdminByRequestService(abrRepo);

            foreach (AdminByRequestSetting setting in _settings)
            {
                abrService.CreateSetting(setting.Name, setting.ActiveDirectoryGroups);
                _logger.LogInfo(
                    $"Created setting: {setting.Name} containing {setting.ActiveDirectoryGroups.Count} AD-groups");
            }

            // Combine data into CompletedUser list
            _logger.LogInfo("Building Completed Users to prepare analysis...");
            var dataHandler = new DataHandler(
                abrRepo.LoadCachedAuditLogs(_auditCachePath),
                abrRepo.LoadCachedInventoryData(_inventoryCachePath),
                adRepo,
                _logger
            );
            List<CompletedUser> completedUsers = dataHandler.BuildCompletedUsers();

            _logger.LogInfo($"[CACHE] Built {completedUsers.Count} CompletedUsers from the data.");

            // Compute all stats using the injected Analyzer
            OverallStatisticsResult overallStats = _analyzer.ComputeOverallStatistics(completedUsers, _settings);
            List<UnusedAdGroupResult> unusedGroups = _analyzer.ComputeUnusedAdGroups(completedUsers, _settings, adRepo);
            Dictionary<string, ApplicationStatisticsResult> appStats =
                _analyzer.ComputeApplicationStatistics(completedUsers, _settings);
            List<TerminalStatisticsRow> terminalStats = _analyzer.ComputeTerminalStatistics(completedUsers, _settings);

            // Generate HTML report
            var htmlWriter = new HtmlReportWriter();
            var htmlContent = htmlWriter.BuildHtmlReport(
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
            // Handle specific exceptions from Analyzer or other classes
            _logger.LogError($"Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Handle unexpected exceptions
            _logger.LogError($"An unexpected error occurred: {ex.Message}");
        }
    }
}
