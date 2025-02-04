using Microsoft.Extensions.Configuration;
using SecureLens.Application.Analysis.Results;
using SecureLens.Application.Services.Interfaces;
using SecureLens.Core.Models;
using SecureLens.Infrastructure.Factories;
using SecureLens.Infrastructure.Interfaces;
using SecureLens.Infrastructure.Logging;
using SecureLens.Utilities;

namespace SecureLens.Application.Services;

public class OnlineModeHandler(
    ILogger logger,
    List<AdminByRequestSetting> settings,
    char[] apiKey, // Changed from string to char[]
    IConfiguration configuration,
    Analyzer analyzer)
    : IModeHandler
{
    private readonly string? _reportPath = configuration.GetValue<string>("ReportPath");
    // private readonly string _groupCachePath = configuration.GetValue<string>("CachePaths:GroupCache");
    // private readonly string _userCachePath = configuration.GetValue<string>("CachePaths:UserCache");
    // private readonly IConfiguration _configuration = configuration;

    // Injected Analyzer

    public async Task ExecuteAsync()
    {
        try
        {
            IAdminByRequestRepository abrRepo = RepositoryFactory.CreateAdminByRequestRepository(
                new string(apiKey), // Convert char[] to string when passing to repository
                logger,
                true
            );

            IActiveDirectoryRepository adRepo = RepositoryFactory.CreateActiveDirectoryRepository(
                logger,
                true
            );

            var abrService = new AdminByRequestService(abrRepo);

            foreach (AdminByRequestSetting setting in settings)
            {
                abrService.CreateSetting(setting.Name, setting.ActiveDirectoryGroups);
                logger.LogInfo(
                    $"Created setting: {setting.Name} containing {setting.ActiveDirectoryGroups.Count} AD-groups");
            }

            // Fetch Inventory Data (online)
            logger.LogInfo("=== Inventory Data ===");
            List<InventoryLogEntry> inventory = await abrRepo.FetchInventoryDataAsync();

            logger.LogInfo(inventory.Count > 0
                ? $"Fetched {inventory.Count} inventory logs (online)."
                : "No inventory data fetched (online).");

            // Fetch Audit Logs (online)
            logger.LogInfo("=== Audit Logs ===");
            Dictionary<string, string> auditParams = new()
            {
                { "take", "1" }, // Adjust as needed
                { "wantscandetails", "1" },
                { "startdate", "2024-01-01" },
                { "enddate", "2025-12-31" },
                { "status", "Finished" },
                { "type", "app" }
            };
            List<AuditLogEntry> auditLogs = await abrRepo.FetchAuditLogsAsync(auditParams);
            logger.LogInfo(auditLogs.Count > 0
                ? $"Fetched {auditLogs.Count} audit logs (online)."
                : "No audit log data fetched (online).");

            // Combine all in CompletedUser list
            logger.LogInfo("Building Completed Users to prepare analysis...");
            var dataHandler = new DataHandler(auditLogs, inventory, adRepo, logger);
            List<CompletedUser> completedUsers = dataHandler.BuildCompletedUsers();

            logger.LogInfo($"[ONLINE] Built {completedUsers.Count} CompletedUsers from the data.");

            // Calculate statistics using the injected Analyzer
            OverallStatisticsResult overallStats = analyzer.ComputeOverallStatistics(completedUsers, settings);
            List<UnusedAdGroupResult> unusedGroups = analyzer.ComputeUnusedAdGroups(completedUsers, settings, adRepo);
            Dictionary<string, ApplicationStatisticsResult> appStats =
                analyzer.ComputeApplicationStatistics(completedUsers, settings);
            List<TerminalStatisticsRow> terminalStats = analyzer.ComputeTerminalStatistics(completedUsers, settings);

            // Generate HTML report
            var htmlWriter = new HtmlReportWriter();
            var htmlContent = htmlWriter.BuildHtmlReport(
                overallStats,
                appStats,
                terminalStats,
                unusedGroups,
                settings
            );

            // Write HTML report to file
            await File.WriteAllTextAsync(_reportPath, htmlContent);
            logger.LogInfo($"[INFO] HTML report successfully written to '{_reportPath}'.");
        }
        catch (ArgumentNullException ex)
        {
            logger.LogError($"Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Handle unexpected errors
            logger.LogError($"An unexpected error occurred: {ex.Message}");
        }
    }
}