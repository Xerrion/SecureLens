using SecureLens.Application.Analysis.Interfaces;
using SecureLens.Application.Analysis.Results;
using SecureLens.Core.Models;
using SecureLens.Infrastructure.Interfaces;

namespace SecureLens.Application;

public class Analyzer(
    IOverallStatisticsCalculator overallStatsCalculator,
    IApplicationStatisticsCalculator appStatsCalculator,
    ITerminalStatisticsCalculator terminalStatsCalculator,
    IUnusedAdGroupsCalculator unusedAdGroupsCalculator)
{
    public OverallStatisticsResult ComputeOverallStatistics(List<CompletedUser> completedUsers,
        List<AdminByRequestSetting> settings)
    {
        return overallStatsCalculator.ComputeOverallStatistics(completedUsers, settings);
    }

    public Dictionary<string, ApplicationStatisticsResult> ComputeApplicationStatistics(
        List<CompletedUser> completedUsers, List<AdminByRequestSetting> settings)
    {
        return appStatsCalculator.ComputeApplicationStatistics(completedUsers, settings);
    }

    public List<TerminalStatisticsRow> ComputeTerminalStatistics(List<CompletedUser> completedUsers,
        List<AdminByRequestSetting> settings)
    {
        return terminalStatsCalculator.ComputeTerminalStatistics(completedUsers, settings);
    }

    public List<UnusedAdGroupResult> ComputeUnusedAdGroups(List<CompletedUser> completedUsers,
        List<AdminByRequestSetting> settings, IActiveDirectoryRepository adRepo)
    {
        return unusedAdGroupsCalculator.ComputeUnusedAdGroups(completedUsers, settings, adRepo);
    }
}