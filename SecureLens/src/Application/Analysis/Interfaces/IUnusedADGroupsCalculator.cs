using SecureLens.Application.Analysis.Results;
using SecureLens.Core.Models;
using SecureLens.Infrastructure.Interfaces;

namespace SecureLens.Application.Analysis.Interfaces;

public interface IUnusedAdGroupsCalculator
{
    public List<UnusedAdGroupResult> ComputeUnusedAdGroups(List<CompletedUser> completedUsers,
        List<AdminByRequestSetting> settings, IActiveDirectoryRepository adRepo);
}