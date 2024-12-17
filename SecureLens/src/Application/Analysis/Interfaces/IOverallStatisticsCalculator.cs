using SecureLens.Application.Analysis.Results;
using SecureLens.Core.Models;

namespace SecureLens.Application.Analysis.Interfaces;

public interface IOverallStatisticsCalculator
{
    public OverallStatisticsResult ComputeOverallStatistics(List<CompletedUser> completedUsers,
        List<AdminByRequestSetting> settings);
}