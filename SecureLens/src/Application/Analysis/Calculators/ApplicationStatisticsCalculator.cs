using SecureLens.Application.Analysis.Interfaces;
using SecureLens.Application.Analysis.Results;
using SecureLens.Core.Models;

namespace SecureLens.Application.Analysis.Calculators;

public class ApplicationStatisticsCalculator : IApplicationStatisticsCalculator
{
    public Dictionary<string, ApplicationStatisticsResult> ComputeApplicationStatistics(
        List<CompletedUser> completedUsers, List<AdminByRequestSetting> settings)
    {
        var appStats = new Dictionary<string, ApplicationStatisticsResult>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, List<string>> userSettingsLookup = BuildUserSettingsLookup(completedUsers, settings);

        foreach (CompletedUser user in completedUsers)
        {
            var userSettingNames =
                new HashSet<string>(
                    userSettingsLookup.TryGetValue(user.AccountName, out List<string>? value)
                        ? value
                        : new List<string>(), StringComparer.OrdinalIgnoreCase);

            foreach (AuditLogEntry log in user.AuditLogEntries)
                if (log.Type == "Run As Admin")
                {
                    if (log.Application == null) continue;

                    var appName = log.Application.Name ?? "Unknown Application";
                    var vendor = log.Application.Vendor ?? "Unknown";
                    var preapproved = log.Application.Preapproved ?? false;
                    IncrementAppStats(appStats, userSettingNames, appName, vendor, preapproved);
                }
                else if (log is { Type: "Admin Session", ElevatedApplications: not null })
                {
                    foreach (AuditElevatedApplication elevatedApp in log.ElevatedApplications)
                    {
                        var appName = elevatedApp.Name ?? "Unknown Application";
                        var vendor = elevatedApp.Vendor ?? "Unknown";
                        const bool preapproved = false;
                        IncrementAppStats(appStats, userSettingNames, appName, vendor, preapproved);
                    }
                }
        }

        return appStats;
    }

    private static Dictionary<string, List<string>> BuildUserSettingsLookup(List<CompletedUser> completedUsers,
        List<AdminByRequestSetting> settings)
    {
        var userSettings = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (CompletedUser user in completedUsers)
        {
            userSettings[user.AccountName] = new List<string>();
            if (user.ActiveDirectoryUser == null)
                continue;

            var groupNames = user.ActiveDirectoryUser.Groups
                .Select(g => g.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (AdminByRequestSetting s in settings)
            {
                var belongs =
                    groupNames.Any(g => s.ActiveDirectoryGroups.Contains(g, StringComparer.OrdinalIgnoreCase));
                if (belongs) userSettings[user.AccountName].Add(s.Name);
            }
        }

        return userSettings;
    }

    private static void IncrementAppStats(
        Dictionary<string, ApplicationStatisticsResult> appStats,
        HashSet<string> userSettingNames,
        string appName,
        string vendor,
        bool preapproved)
    {
        if (!appStats.TryGetValue(appName, out ApplicationStatisticsResult? value))
        {
            value = new ApplicationStatisticsResult
            {
                Vendor = vendor,
                Preapproved = preapproved,
                TotalCount = 0,
                TechnologyCount = 0,
                ElevateTerminalRightsCount = 0,
                GlobalCount = 0
            };
            appStats[appName] = value;
        }

        if (preapproved) value.Preapproved = true;
        if (value.Vendor == "Unknown" && vendor != "Unknown") value.Vendor = vendor;

        value.TotalCount++;

        if (userSettingNames.Contains("Technology")) value.TechnologyCount++;
        if (userSettingNames.Contains("Elevate Terminal Rights")) value.ElevateTerminalRightsCount++;
        if (userSettingNames.Contains("Global")) value.GlobalCount++;
    }
}