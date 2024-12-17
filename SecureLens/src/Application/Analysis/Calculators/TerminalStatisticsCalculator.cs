using Microsoft.Extensions.Configuration;
using SecureLens.Application.Analysis.Interfaces;
using SecureLens.Application.Analysis.Results;
using SecureLens.Core.Models;

namespace SecureLens.Application.Analysis.Calculators;

public class TerminalStatisticsCalculator : ITerminalStatisticsCalculator
{
    private readonly HashSet<string> _knownTerminalApps;

    public TerminalStatisticsCalculator(IConfiguration configuration)
    {
        List<string>? apps = configuration.GetSection("KnownTerminalApps").Get<List<string>>();
        if (apps != null) _knownTerminalApps = new HashSet<string>(apps, StringComparer.OrdinalIgnoreCase);
    }

    public List<TerminalStatisticsRow> ComputeTerminalStatistics(List<CompletedUser> completedUsers,
        List<AdminByRequestSetting> settings)
    {
        // user -> (appName -> count)
        var userTerminalApps = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

        // Build quick lookups for user’s department, title, AD groups, settings
        var userDepartments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var userTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var userAdGroups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, List<string>>
            userSettings = BuildUserSettingsLookup(completedUsers, settings); // user => list<settings>

        foreach (CompletedUser user in completedUsers)
        {
            userTerminalApps[user.AccountName] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (user.ActiveDirectoryUser != null)
            {
                userDepartments[user.AccountName] = user.ActiveDirectoryUser.Department ?? "N/A";
                userTitles[user.AccountName] = user.ActiveDirectoryUser.Title ?? "N/A";
                var groups = user.ActiveDirectoryUser.Groups.Select(g => g.Name).ToList();
                userAdGroups[user.AccountName] = groups;
            }
            else
            {
                userDepartments[user.AccountName] = "N/A";
                userTitles[user.AccountName] = "N/A";
                userAdGroups[user.AccountName] = new List<string>();
            }
        }

        // Parse AuditLogs for recognized terminal apps
        foreach (CompletedUser user in completedUsers)
        {
            Dictionary<string, int> appsForUser = userTerminalApps[user.AccountName];

            foreach (AuditLogEntry entry in user.AuditLogEntries)
                if (entry.Type == "Run As Admin" && entry.Application != null)
                {
                    var appName = entry.Application.Name ?? "";
                    if (IsTerminalApp(appName))
                    {
                        if (!appsForUser.ContainsKey(appName))
                            appsForUser[appName] = 0;
                        appsForUser[appName]++;
                    }
                }
                else if (entry.Type == "Admin Session" && entry.ElevatedApplications != null)
                {
                    foreach (AuditElevatedApplication elevatedApp in entry.ElevatedApplications)
                    {
                        var appName = elevatedApp.Name ?? "";
                        if (IsTerminalApp(appName))
                        {
                            if (!appsForUser.ContainsKey(appName))
                                appsForUser[appName] = 0;
                            appsForUser[appName]++;
                        }
                    }
                }
        }

        var result = new List<TerminalStatisticsRow>();
        foreach (CompletedUser user in completedUsers)
        {
            Dictionary<string, int> apps = userTerminalApps[user.AccountName];
            if (apps.Count == 0)
                continue;

            var totalCount = 0;
            var appStrings = new List<string>();
            foreach (KeyValuePair<string, int> kvp in apps.OrderByDescending(k => k.Value))
            {
                appStrings.Add($"{kvp.Key} ({kvp.Value})");
                totalCount += kvp.Value;
            }

            var applicationsStr = string.Join(", ", appStrings);

            List<string> settingNames = userSettings.TryGetValue(user.AccountName, out List<string>? value)
                ? value
                : new List<string>();
            settingNames.Sort(StringComparer.OrdinalIgnoreCase);
            var settingsUsed = settingNames.Count > 0 ? string.Join(", ", settingNames) : "N/A";

            List<string> groups = userAdGroups.ContainsKey(user.AccountName)
                ? userAdGroups[user.AccountName]
                : new List<string>();
            groups.Sort(StringComparer.OrdinalIgnoreCase);
            var sourceAdGroups = groups.Count > 0 ? string.Join(", ", groups) : "N/A";

            var row = new TerminalStatisticsRow // Korrekt type
            {
                User = user.AccountName,
                Applications = applicationsStr,
                Count = totalCount,
                Department = userDepartments.ContainsKey(user.AccountName) ? userDepartments[user.AccountName] : "N/A",
                Title = userTitles.ContainsKey(user.AccountName) ? userTitles[user.AccountName] : "N/A",
                SettingsUsed = settingsUsed,
                Types = "Run As Admin",
                SourceADGroups = sourceAdGroups
            };
            result.Add(row);
        }

        // Sort descending by count
        return result.OrderByDescending(r => r.Count).ToList();
    }

    private bool IsTerminalApp(string appName)
    {
        return _knownTerminalApps.Contains(appName.Trim());
    }

    private Dictionary<string, List<string>> BuildUserSettingsLookup(List<CompletedUser> completedUsers,
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
}