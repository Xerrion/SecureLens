using SecureLens.Analysis.Results;

namespace SecureLens.Analysis;

public interface IUnusedAdGroupsCalculator
{
    List<UnusedAdGroupResult> ComputeUnusedAdGroups(List<CompletedUser> completedUsers, List<AdminByRequestSetting> settings, IActiveDirectoryRepository adRepo);
}