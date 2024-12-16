using SecureLens.Analysis.Results;
using SecureLens.Models;
using SecureLens.Data;
using System.Collections.Generic;
using System.Linq;

namespace SecureLens.Analysis
{
    public class ApplicationStatisticsCalculator : IApplicationStatisticsCalculator
    {
        public Dictionary<string, ApplicationStatisticsResult> ComputeApplicationStatistics(List<CompletedUser> completedUsers, List<AdminByRequestSetting> settings)
        {
            var appStats = new Dictionary<string, ApplicationStatisticsResult>(StringComparer.OrdinalIgnoreCase);
            var userSettingsLookup = BuildUserSettingsLookup(completedUsers, settings);

            foreach (var user in completedUsers)
            {
                var userSettingNames = new HashSet<string>(userSettingsLookup.ContainsKey(user.AccountName) ? userSettingsLookup[user.AccountName] : new List<string>(), StringComparer.OrdinalIgnoreCase);

                foreach (var log in user.AuditLogEntries)
                {
                    if (log.Type == "Run As Admin")
                    {
                        if (log.Application == null) continue;

                        string appName = log.Application.Name ?? "Unknown Application";
                        string vendor = log.Application.Vendor ?? "Unknown";
                        bool preapproved = log.Application.Preapproved ?? false;
                        IncrementAppStats(appStats, userSettingNames, appName, vendor, preapproved);
                    }
                    else if (log.Type == "Admin Session")
                    {
                        if (log.ElevatedApplications != null)
                        {
                            foreach (var elevatedApp in log.ElevatedApplications)
                            {
                                string appName = elevatedApp.Name ?? "Unknown Application";
                                string vendor = elevatedApp.Vendor ?? "Unknown";
                                bool preapproved = false;
                                IncrementAppStats(appStats, userSettingNames, appName, vendor, preapproved);
                            }
                        }
                    }
                }
            }

            return appStats;
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

        private void IncrementAppStats(
            Dictionary<string, ApplicationStatisticsResult> appStats,
            HashSet<string> userSettingNames,
            string appName,
            string vendor,
            bool preapproved)
        {
            if (!appStats.ContainsKey(appName))
            {
                appStats[appName] = new ApplicationStatisticsResult
                {
                    Vendor = vendor,
                    Preapproved = preapproved,
                    TotalCount = 0,
                    TechnologyCount = 0,
                    ElevateTerminalRightsCount = 0,
                    GlobalCount = 0
                };
            }

            if (preapproved) 
            {
                appStats[appName].Preapproved = true;
            }
            if (appStats[appName].Vendor == "Unknown" && vendor != "Unknown")
            {
                appStats[appName].Vendor = vendor;
            }

            appStats[appName].TotalCount++;

            if (userSettingNames.Contains("Technology"))
            {
                appStats[appName].TechnologyCount++;
            }
            if (userSettingNames.Contains("Elevate Terminal Rights"))
            {
                appStats[appName].ElevateTerminalRightsCount++;
            }
            if (userSettingNames.Contains("Global"))
            {
                appStats[appName].GlobalCount++;
            }
        }
    }
}
