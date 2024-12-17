using SecureLens.Application.Analysis.Interfaces;
using SecureLens.Application.Analysis.Results;
using SecureLens.Core.Models;

namespace SecureLens.Application.Analysis.Calculators;

public class OverallStatisticsCalculator : IOverallStatisticsCalculator
{
    public OverallStatisticsResult ComputeOverallStatistics(List<CompletedUser> completedUsers,
        List<AdminByRequestSetting> settings)
    {
        var result = new OverallStatisticsResult
        {
            MembersPerSetting = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
            DepartmentsUnderSetting = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase),
            TitlesUnderSetting = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase),
            TotalUniqueUsers = completedUsers.Count
        };

        var workstationSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (CompletedUser user in completedUsers)
        foreach (InventoryLogEntry inv in user.InventoryLogEntries)
            if (!string.IsNullOrEmpty(inv.Name))
                workstationSet.Add(inv.Name);

        result.TotalUniqueWorkstations = workstationSet.Count;

        var settingMembership = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (AdminByRequestSetting s in settings)
            settingMembership[s.Name] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (CompletedUser user in completedUsers)
        {
            if (user.ActiveDirectoryUser == null) continue;

            var groupNames = user.ActiveDirectoryUser.Groups
                .Select(g => g.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (AdminByRequestSetting s in settings)
            {
                var belongs =
                    groupNames.Any(g => s.ActiveDirectoryGroups.Contains(g, StringComparer.OrdinalIgnoreCase));
                if (belongs) settingMembership[s.Name].Add(user.AccountName);
            }
        }

        foreach (AdminByRequestSetting s in settings)
        {
            result.MembersPerSetting[s.Name] = settingMembership[s.Name].Count;
            result.DepartmentsUnderSetting[s.Name] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            result.TitlesUnderSetting[s.Name] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        // (4) Departments and Titles per setting
        foreach (CompletedUser user in completedUsers)
        {
            if (user.ActiveDirectoryUser == null) continue;

            var userDept = user.ActiveDirectoryUser.Department ?? "";
            var userTitle = user.ActiveDirectoryUser.Title ?? "";
            var groupNames = user.ActiveDirectoryUser.Groups
                .Select(g => g.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (AdminByRequestSetting s in settings)
            {
                var belongs =
                    groupNames.Any(g => s.ActiveDirectoryGroups.Contains(g, StringComparer.OrdinalIgnoreCase));
                if (belongs)
                {
                    if (!string.IsNullOrEmpty(userDept))
                        result.DepartmentsUnderSetting[s.Name].Add(userDept);

                    if (!string.IsNullOrEmpty(userTitle))
                        result.TitlesUnderSetting[s.Name].Add(userTitle);
                }
            }
        }

        // (5) Users with >= 5 elevations
        var usersWith5 = 0;
        foreach (CompletedUser user in completedUsers)
        {
            var userElevationCount = user.AuditLogEntries
                .Count(a => a.Type == "Run As Admin" || a.Type == "Admin Session");
            if (userElevationCount >= 5)
                usersWith5++;
        }

        result.UsersWith5Elevations = usersWith5;

        return result;
    }
}