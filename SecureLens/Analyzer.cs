using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureLens
{
    public class Analyzer
    {
        private readonly List<CompletedUser> _completedUsers;
        private readonly List<AdminByRequestSetting> _settings;

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

        /// <summary>
        /// For "Unused AD Groups": which group, which setting, how many members.
        /// </summary>
        public class UnusedAdGroupResult
        {
            public string ADGroup { get; set; }
            public string Setting { get; set; }
            public int NumberOfUsers { get; set; }
        }

        #endregion

        #region OverallStatistics (existing logic)

        public OverallStatisticsResult ComputeOverallStatistics()
        {
            var result = new OverallStatisticsResult
            {
                MembersPerSetting = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
                DepartmentsUnderSetting = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase),
                TitlesUnderSetting = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            };

            // 1) total unique users
            result.TotalUniqueUsers = _completedUsers.Count;

            // 2) total unique workstations
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

            // 3) figure out membership for each setting
            var settingMembership = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in _settings)
            {
                settingMembership[s.Name] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            // assign user to each setting based on AD group intersection
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

            // store final membership counts
            foreach (var s in _settings)
            {
                result.MembersPerSetting[s.Name] = settingMembership[s.Name].Count;
                result.DepartmentsUnderSetting[s.Name] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                result.TitlesUnderSetting[s.Name] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            // 4) gather departments/titles for each setting
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

            // 5) count how many users have >= 5 elevations
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

        public void PrintOverallStatistics(OverallStatisticsResult stats)
        {
            var rows = new List<string[]>
            {
                new string[] { "Total unique users in environment:", stats.TotalUniqueUsers.ToString() },
                new string[] { "Total unique workstations:", stats.TotalUniqueWorkstations.ToString() }
            };

            foreach (var s in _settings)
            {
                rows.Add(new string[] 
                { 
                    $"Total members able to use '{s.Name}':", 
                    stats.MembersPerSetting[s.Name].ToString() 
                });
            }

            rows.Add(new string[]
            {
                "Users with at least 5 elevations in period:",
                stats.UsersWith5Elevations.ToString()
            });

            ConsoleTablePrinter.PrintTable("Overall Statistics", new List<string> { "Description", "Count" }, rows, 80);
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

        public void PrintApplicationStatistics(Dictionary<string, ApplicationStatisticsResult> appStats)
        {
            var sortedApps = appStats.OrderByDescending(kvp => kvp.Value.TotalCount).ToList();
            var rows = new List<string[]>();

            foreach (var kvp in sortedApps)
            {
                string appName = kvp.Key;
                var stats = kvp.Value;

                rows.Add(new string[]
                {
                    stats.TotalCount.ToString(),
                    appName,
                    stats.Vendor,
                    stats.Preapproved ? "True" : "False",
                    stats.TechnologyCount.ToString(),
                    stats.ElevateTerminalRightsCount.ToString(),
                    stats.GlobalCount.ToString()
                });
            }

            var columns = new List<string>
            {
                "Total Count",
                "Application",
                "Vendor",
                "Pre-approved",
                "'Technology' Count",
                "'Elevate Terminal Rights' Count",
                "'Global' Count"
            };

            ConsoleTablePrinter.PrintTable("Application Statistics", columns, rows, 120);
        }

        #endregion

        #region Unused AD Groups

        /// <summary>
        /// Finds AD groups (under the relevant settings) that had no elevations. 
        /// For each "unused" group, also returns the number of users in that group. 
        /// 
        /// The logic: 
        /// 1) For each setting, look at each AD group in AdminByRequestSetting.ActiveDirectoryGroups. 
        /// 2) If no user from that group had any app/session elevation, the group is "unused." 
        /// 3) "NumberOfUsers" can come from the AD user cache or from CompletedUsers, 
        ///    but typically from AD if we want the total members. 
        ///    Or if we just want the intersection with CompletedUsers, adapt logic accordingly.
        /// </summary>
        /// <param name="adClient">We rely on AD data to see how many users are in each group.</param>
        public List<UnusedAdGroupResult> ComputeUnusedAdGroups(ActiveDirectoryClient adClient)
        {
            var results = new List<UnusedAdGroupResult>();

            if (adClient == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning: ActiveDirectoryClient is null. Cannot compute unused AD groups.");
                Console.ResetColor();
                return results;
            }

            // 1) Identify which AD groups are actually "used." 
            //    A group is "used" if any user from that group had at least 1 elevation.
            var usedGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Build a helper: user => the AD group names they belong to
            var userToGroups = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var user in _completedUsers)
            {
                var userAccount = user.AccountName; // already normalized
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

            // Step 2) For each user, if the user had any elevations, mark their groups as "used".
            foreach (var user in _completedUsers)
            {
                int elevationCount = user.AuditLogEntries
                                         .Count(a => a.Type == "Run As Admin" || a.Type == "Admin Session");
                if (elevationCount > 0)
                {
                    // user has elevations => all AD groups of this user are used
                    foreach (var g in userToGroups[user.AccountName])
                    {
                        usedGroups.Add(g);
                    }
                }
            }

            // 3) For each setting, for each AD group in that setting, check if it is in usedGroups
            //    If not, it's "unused." We then figure out how many members are in that group via adClient or userToGroups
            foreach (var setting in _settings)
            {
                // We'll only do this for "Elevate Terminal Rights" and "Technology" 
                // if you want it specifically. If you want to include "Global," do so as well.
                // But let's say the question specifically mentions: "Unused AD Groups under 'Elevate Terminal Rights' and 'Technology'"
                // Then we skip 'Global' or others if needed. 
                if (setting.Name == "Global")
                {
                    // If you want to skip Global, uncomment:
                    //continue;
                }

                foreach (var groupName in setting.ActiveDirectoryGroups)
                {
                    if (!usedGroups.Contains(groupName))
                    {
                        // Unused group
                        // Next find how many users are in that group (via AD client groupCache or so).
                        // We'll do "QueryAdGroupFromCache" or "CollectAdGroupMembersFromCache"
                        // The user wants "Number of Users" which presumably is the count of that group from cache
                        var members = adClient.QueryAdGroupFromCache(groupName);
                        int numberOfUsers = members.Count;

                        // Add to results
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

        /// <summary>
        /// Print unused AD groups in a table, sorting by Setting and group name, for example.
        /// </summary>
        public void PrintUnusedAdGroups(List<UnusedAdGroupResult> unusedGroups)
        {
            if (unusedGroups == null || unusedGroups.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("All AD groups under these settings are involved in elevations. No unused groups found.");
                Console.ResetColor();
                return;
            }

            var sorted = unusedGroups
                .OrderBy(u => u.Setting, StringComparer.OrdinalIgnoreCase)
                .ThenBy(u => u.ADGroup, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var rows = new List<string[]>();
            foreach (var item in sorted)
            {
                rows.Add(new string[]
                {
                    item.ADGroup,
                    item.Setting,
                    item.NumberOfUsers.ToString()
                });
            }

            var columns = new List<string> { "AD Group", "Setting", "Number of Users" };
            ConsoleTablePrinter.PrintTable("Unused AD Groups under 'Elevate Terminal Rights' and 'Technology'", 
                                            columns, rows, 80);
        }

        #endregion
    }
}
