using SecureLens.Analysis.Results;

namespace SecureLens.Analysis;

public interface ITerminalStatisticsCalculator
{
    List<TerminalStatisticsRow> ComputeTerminalStatistics(List<CompletedUser> completedUsers, List<AdminByRequestSetting> settings);
}