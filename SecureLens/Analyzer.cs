using SecureLens.Models;

namespace SecureLens
{
    public class Analyzer
    {
        private readonly List<CompletedUser> _completedUsers;
        private readonly List<AdminByRequestSetting> _settings;

        // You can adapt these to your actual recognized terminal apps, 
        // ensuring the string values match how they're logged in your environment:
        private static readonly HashSet<string> KnownTerminalApps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "cmd.exe",
            "PowerShell",
            "Windows Command Processor",
            "windows powershell ise",
            "windows subsystem for linux",
            "git for windows"
        };

        public Analyzer(List<CompletedUser> completedUsers, List<AdminByRequestSetting> settings)
        {
            _completedUsers = completedUsers ?? new List<CompletedUser>();
            _settings = settings ?? new List<AdminByRequestSetting>();
        }

        #region Data Structures

        public class OverallStatisticsResult
        {
            public int TotalUniqueUsers { get; set; }
            public int TotalUniqueWorkstations { get; set; }
            public Dictionary<string, int> MembersPerSetting { get; set; }
            public Dictionary<string, HashSet<string>> DepartmentsUnderSetting { get; set; }
            public Dictionary<string, HashSet<string>> TitlesUnderSetting { get; set; }
            public int UsersWith5Elevations { get; set; }
        }

        public class ApplicationStatisticsResult
        {
            public string Vendor { get; set; }
            public bool Preapproved { get; set; }
            public int TotalCount { get; set; }
            public int TechnologyCount { get; set; }
            public int ElevateTerminalRightsCount { get; set; }
            public int GlobalCount { get; set; }
        }

        public class UnusedAdGroupResult
        {
            public string ADGroup { get; set; }
            public string Setting { get; set; }
            public int NumberOfUsers { get; set; }
        }

        /// <summary>
        /// Row structure for the final "Terminal Statistics" table.
        /// Matches your Python PoC output columns.
        /// </summary>
        public class TerminalStatisticsRow
        {
            public string User { get; set; }
            public string Applications { get; set; }
            public int Count { get; set; }
            public string Department { get; set; }
            public string Title { get; set; }
            public string SettingsUsed { get; set; }
            public string Types { get; set; }
            public string SourceADGroups { get; set; }
        }

        #endregion

        #region OverallStatistics

        public OverallStatisticsResult ComputeOverallStatistics()
        {
            var result = new OverallStatisticsResult
            {
                MembersPerSetting = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
                DepartmentsUnderSetting = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase),
                TitlesUnderSetting = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            };

            // (1) Total unique users
            result.TotalUniqueUsers = _completedUsers.Count;

            // (2) Total unique workstations
            var workstationSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var user in _completedUsers)
            {
                foreach (var inv in user.InventoryLogEntries)
                {
                    if (!string.IsNullOrEmpty(inv?.Name))
                        workstationSet.Add(inv.Name);
                }
            }
            result.TotalUniqueWorkstations = workstationSet.Count;

            // (3) Membership for each setting
            var settingMembership = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in _settings)
            {
                settingMembership[s.Name] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            foreach (var user in _completedUsers)
            {
                if (user.ActiveDirectoryUser == null) continue;

                var groupNames = user.ActiveDirectoryUser.Groups
                                        .Select(g => g.Name)
                                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var s in _settings)
                {
                    bool belongs = groupNames.Any(g => s.ActiveDirectoryGroups.Contains(g, StringComparer.OrdinalIgnoreCase));
                    if (belongs)
                    {
                        settingMembership[s.Name].Add(user.AccountName);
                    }
                }
            }

            foreach (var s in _settings)
            {
                result.MembersPerSetting[s.Name] = settingMembership[s.Name].Count;
                result.DepartmentsUnderSetting[s.Name] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                result.TitlesUnderSetting[s.Name] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            // (4) Departments and Titles per setting
            foreach (var user in _completedUsers)
            {
                if (user.ActiveDirectoryUser == null) continue;

                string userDept = user.ActiveDirectoryUser.Department ?? "";
                string userTitle = user.ActiveDirectoryUser.Title ?? "";
                var groupNames = user.ActiveDirectoryUser.Groups
                                      .Select(g => g.Name)
                                      .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var s in _settings)
                {
                    bool belongs = groupNames.Any(g => s.ActiveDirectoryGroups.Contains(g, StringComparer.OrdinalIgnoreCase));
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
            int usersWith5 = 0;
            foreach (var user in _completedUsers)
            {
                int userElevationCount = user.AuditLogEntries
                                             .Count(a => a.Type == "Run As Admin" || a.Type == "Admin Session");
                if (userElevationCount >= 5)
                    usersWith5++;
            }
            result.UsersWith5Elevations = usersWith5;

            return result;
        }

        #endregion

        #region ApplicationStatistics

        public Dictionary<string, ApplicationStatisticsResult> ComputeApplicationStatistics()
        {
            var appStats = new Dictionary<string, ApplicationStatisticsResult>(StringComparer.OrdinalIgnoreCase);
            var userSettings = BuildUserSettingsLookup();

            foreach (var user in _completedUsers)
            {
                var userSettingNames = new HashSet<string>(userSettings[user.AccountName] ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

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

        private Dictionary<string, List<string>> BuildUserSettingsLookup()
        {
            var userSettings = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var user in _completedUsers)
            {
                userSettings[user.AccountName] = new List<string>();
                if (user.ActiveDirectoryUser == null) 
                    continue;

                var groupNames = user.ActiveDirectoryUser.Groups
                                        .Select(g => g.Name)
                                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var s in _settings)
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

        #endregion

        #region TerminalStatistics

        /// <summary>
        /// Computes who ran which "terminal" applications. 
        /// For each user that ran terminal apps, builds a TerminalStatisticsRow with:
        /// - User
        /// - Aggregated Applications + counts
        /// - Department, Title, Settings used
        /// - "Run As Admin" type
        /// - Source AD Groups
        /// </summary>
        public List<TerminalStatisticsRow> ComputeTerminalStatistics()
        {
            // user -> (appName -> count)
            var userTerminalApps = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

            // Build quick lookups for user’s department, title, AD groups, settings
            var userDepartments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var userTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var userAdGroups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var userSettings = BuildUserSettingsLookup();  // user => list<settings>

            foreach (var user in _completedUsers)
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
            foreach (var user in _completedUsers)
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
            var result = new List<TerminalStatisticsRow>();
            foreach (var user in _completedUsers)
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

                var settingNames = userSettings[user.AccountName];
                settingNames.Sort(StringComparer.OrdinalIgnoreCase);
                string settingsUsed = settingNames.Count > 0 ? string.Join(", ", settingNames) : "N/A";

                var groups = userAdGroups[user.AccountName];
                groups.Sort(StringComparer.OrdinalIgnoreCase);
                string sourceAdGroups = groups.Count > 0 ? string.Join(", ", groups) : "N/A";

                var row = new TerminalStatisticsRow
                {
                    User = user.AccountName,
                    Applications = applicationsStr,
                    Count = totalCount,
                    Department = userDepartments[user.AccountName],
                    Title = userTitles[user.AccountName],
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
            return KnownTerminalApps.Contains(appName.Trim());
        }

        #endregion

        #region UnusedAdGroups

        public List<UnusedAdGroupResult> ComputeUnusedAdGroups(IActiveDirectoryRepository adRepo)
        {
            var results = new List<UnusedAdGroupResult>();

            if (adRepo == null)
                throw new ArgumentNullException(nameof(adRepo), "ActiveDirectoryRepository is null. Cannot compute unused AD groups.");

            var usedGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var userToGroups = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var user in _completedUsers)
            {
                var userAccount = user.AccountName;
                userToGroups[userAccount] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (user.ActiveDirectoryUser != null)
                {
                    var userGroups = user.ActiveDirectoryUser.Groups.Select(g => g.Name);
                    foreach (var grp in userGroups)
                    {
                        userToGroups[userAccount].Add(grp);
                    }
                }
            }

            // Mark groups as used if any user has elevations
            foreach (var user in _completedUsers)
            {
                int elevationCount = user.AuditLogEntries
                                         .Count(a => a.Type == "Run As Admin" || a.Type == "Admin Session");
                if (elevationCount > 0)
                {
                    foreach (var g in userToGroups[user.AccountName])
                    {
                        usedGroups.Add(g);
                    }
                }
            }

            // Identify which groups from the settings are not in usedGroups
            foreach (var setting in _settings)
            {
                foreach (var groupName in setting.ActiveDirectoryGroups)
                {
                    if (!usedGroups.Contains(groupName))
                    {
                        var members = adRepo.QueryAdGroupFromFile(groupName);
                        int numberOfUsers = members.Count;

                        results.Add(new UnusedAdGroupResult
                        {
                            ADGroup = groupName,
                            Setting = setting.Name,
                            NumberOfUsers = numberOfUsers
                        });
                    }
                }
            }

            return results;
        }

        #endregion
    }
}
