using Microsoft.Extensions.Configuration;
using SecureLens.Analysis.Results; // Sørg for at inkludere dette namespace
using SecureLens.Models;
using SecureLens.Data;
using System.Collections.Generic;
using System.Linq;

namespace SecureLens.Analysis
{
    public class TerminalStatisticsCalculator : ITerminalStatisticsCalculator
    {
        private readonly HashSet<string> _knownTerminalApps;

        public TerminalStatisticsCalculator(IConfiguration configuration)
        {
            var apps = configuration.GetSection("KnownTerminalApps").Get<List<string>>();
            _knownTerminalApps = new HashSet<string>(apps, StringComparer.OrdinalIgnoreCase);
        }

        public List<TerminalStatisticsRow> ComputeTerminalStatistics(List<CompletedUser> completedUsers, List<AdminByRequestSetting> settings)
        {
            // user -> (appName -> count)
            var userTerminalApps = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

            // Build quick lookups for user’s department, title, AD groups, settings
            var userDepartments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var userTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var userAdGroups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var userSettings = BuildUserSettingsLookup(completedUsers, settings);  // user => list<settings>

            foreach (var user in completedUsers)
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
            foreach (var user in completedUsers)
            {
                var appsForUser = userTerminalApps[user.AccountName];

                foreach (var entry in user.AuditLogEntries)
                {
                    if (entry.Type == "Run As Admin" && entry.Application != null)
                    {
                        string appName = entry.Application.Name ?? "";
                        if (IsTerminalApp(appName))
                        {
                            if (!appsForUser.ContainsKey(appName))
                                appsForUser[appName] = 0;
                            appsForUser[appName]++;
                        }
                    }
                    else if (entry.Type == "Admin Session" && entry.ElevatedApplications != null)
                    {
                        foreach (var elevatedApp in entry.ElevatedApplications)
                        {
                            string appName = elevatedApp.Name ?? "";
                            if (IsTerminalApp(appName))
                            {
                                if (!appsForUser.ContainsKey(appName))
                                    appsForUser[appName] = 0;
                                appsForUser[appName]++;
                            }
                        }
                    }
                }
            }

            // Convert the dictionary to a list of TerminalStatisticsRow
            var result = new List<TerminalStatisticsRow>(); // Korrekt type
            foreach (var user in completedUsers)
            {
                var apps = userTerminalApps[user.AccountName];
                if (apps.Count == 0) 
                    continue;  // user didn't run recognized terminal apps

                int totalCount = 0;
                var appStrings = new List<string>();
                foreach (var kvp in apps.OrderByDescending(k => k.Value))
                {
                    appStrings.Add($"{kvp.Key} ({kvp.Value})");
                    totalCount += kvp.Value;
                }
                string applicationsStr = string.Join(", ", appStrings);

                var settingNames = userSettings.ContainsKey(user.AccountName) ? userSettings[user.AccountName] : new List<string>();
                settingNames.Sort(StringComparer.OrdinalIgnoreCase);
                string settingsUsed = settingNames.Count > 0 ? string.Join(", ", settingNames) : "N/A";

                var groups = userAdGroups.ContainsKey(user.AccountName) ? userAdGroups[user.AccountName] : new List<string>();
                groups.Sort(StringComparer.OrdinalIgnoreCase);
                string sourceAdGroups = groups.Count > 0 ? string.Join(", ", groups) : "N/A";

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

        private Dictionary<string, List<string>> BuildUserSettingsLookup(List<CompletedUser> completedUsers, List<AdminByRequestSetting> settings)
        {
            var userSettings = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var user in completedUsers)
            {
                userSettings[user.AccountName] = new List<string>();
                if (user.ActiveDirectoryUser == null) 
                    continue;

                var groupNames = user.ActiveDirectoryUser.Groups
                                        .Select(g => g.Name)
                                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var s in settings)
                {
                    bool belongs = groupNames.Any(g => s.ActiveDirectoryGroups.Contains(g, StringComparer.OrdinalIgnoreCase));
                    if (belongs)
                    {
                        userSettings[user.AccountName].Add(s.Name);
                    }
                }
            }
            return userSettings;
        }
    }
}
