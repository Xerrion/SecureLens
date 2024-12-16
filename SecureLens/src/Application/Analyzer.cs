using SecureLens.Application.Analysis.Interfaces;
using SecureLens.Application.Analysis.Results;
using SecureLens.Core.Models;
using SecureLens.Infrastructure.Interfaces;

namespace SecureLens.Application
{
    public class Analyzer
    {
        private readonly IOverallStatisticsCalculator _overallStatsCalculator;
        private readonly IApplicationStatisticsCalculator _appStatsCalculator;
        private readonly ITerminalStatisticsCalculator _terminalStatsCalculator;
        private readonly IUnusedAdGroupsCalculator _unusedAdGroupsCalculator;

        public Analyzer(
            IOverallStatisticsCalculator overallStatsCalculator,
            IApplicationStatisticsCalculator appStatsCalculator,
            ITerminalStatisticsCalculator terminalStatsCalculator,
            IUnusedAdGroupsCalculator unusedAdGroupsCalculator)
        {
            _overallStatsCalculator = overallStatsCalculator;
            _appStatsCalculator = appStatsCalculator;
            _terminalStatsCalculator = terminalStatsCalculator;
            _unusedAdGroupsCalculator = unusedAdGroupsCalculator;
        }

        public OverallStatisticsResult ComputeOverallStatistics(List<CompletedUser> completedUsers, List<AdminByRequestSetting> settings)
        {
            return _overallStatsCalculator.ComputeOverallStatistics(completedUsers, settings);
        }

        public Dictionary<string, ApplicationStatisticsResult> ComputeApplicationStatistics(List<CompletedUser> completedUsers, List<AdminByRequestSetting> settings)
        {
            return _appStatsCalculator.ComputeApplicationStatistics(completedUsers, settings);
        }

        public List<TerminalStatisticsRow> ComputeTerminalStatistics(List<CompletedUser> completedUsers, List<AdminByRequestSetting> settings)
        {
            return _terminalStatsCalculator.ComputeTerminalStatistics(completedUsers, settings);
        }

        public List<UnusedAdGroupResult> ComputeUnusedAdGroups(List<CompletedUser> completedUsers, List<AdminByRequestSetting> settings, IActiveDirectoryRepository adRepo)
        {
            return _unusedAdGroupsCalculator.ComputeUnusedAdGroups(completedUsers, settings, adRepo);
        }
    }
}
