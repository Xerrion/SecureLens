using SecureLens.Analysis.Results;

namespace SecureLens.Analysis
{
    public interface IOverallStatisticsCalculator
    {
        OverallStatisticsResult ComputeOverallStatistics(List<CompletedUser> completedUsers, List<AdminByRequestSetting> settings);
    }
}