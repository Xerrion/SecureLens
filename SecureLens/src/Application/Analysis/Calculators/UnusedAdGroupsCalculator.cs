﻿using SecureLens.Application.Analysis.Interfaces;
using SecureLens.Application.Analysis.Results;
using SecureLens.Core.Models;
using SecureLens.Infrastructure.Interfaces;

namespace SecureLens.Application.Analysis.Calculators;

public class UnusedAdGroupsCalculator : IUnusedAdGroupsCalculator
{
    public List<UnusedAdGroupResult> ComputeUnusedAdGroups(List<CompletedUser> completedUsers,
        List<AdminByRequestSetting> settings, IActiveDirectoryRepository adRepo)
    {
        var results = new List<UnusedAdGroupResult>(); //

        if (adRepo == null)
            throw new ArgumentNullException(nameof(adRepo),
                "ActiveDirectoryRepository is null. Cannot compute unused AD groups.");

        var usedGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var userToGroups = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (CompletedUser user in completedUsers)
        {
            var userAccount = user.AccountName;
            userToGroups[userAccount] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (user.ActiveDirectoryUser != null)
            {
                IEnumerable<string> userGroups = user.ActiveDirectoryUser.Groups.Select(g => g.Name);
                foreach (var grp in userGroups) userToGroups[userAccount].Add(grp);
            }
        }

        // Mark groups as used if any user has elevations
        foreach (CompletedUser user in completedUsers)
        {
            var elevationCount = user.AuditLogEntries
                .Count(a => a.Type == "Run As Admin" || a.Type == "Admin Session");
            if (elevationCount > 0)
                foreach (var g in userToGroups[user.AccountName])
                    usedGroups.Add(g);
        }

        // Identify which groups from the settings are not in usedGroups
        foreach (AdminByRequestSetting setting in settings)
        foreach (var groupName in setting.ActiveDirectoryGroups)
            if (!usedGroups.Contains(groupName))
            {
                List<string> members = adRepo.QueryAdGroup(groupName);
                var numberOfUsers = members.Count;

                results.Add(new UnusedAdGroupResult // Korrekt type
                {
                    ADGroup = groupName,
                    Setting = setting.Name,
                    NumberOfUsers = numberOfUsers
                });
            }

        return results;
    }
}