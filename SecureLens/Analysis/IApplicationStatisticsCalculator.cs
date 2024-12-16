using SecureLens.Analysis.Results;
using SecureLens.Models;
using SecureLens.Data;
using System.Collections.Generic;

namespace SecureLens.Analysis
{
    public interface IApplicationStatisticsCalculator
    {
        Dictionary<string, ApplicationStatisticsResult> ComputeApplicationStatistics(List<CompletedUser> completedUsers, List<AdminByRequestSetting> settings);
    }
}