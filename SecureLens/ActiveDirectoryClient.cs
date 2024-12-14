using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;

namespace SecureLens
{
    public class ActiveDirectoryClient
    {
        // Her gemmes caching-data i to dictionaries:
        // 1) groupCache: groupName -> liste af userIDs
        // 2) userCache:  userID    -> AdUser-model
        private Dictionary<string, List<string>> groupCache = null;
        private Dictionary<string, ActiveDirectoryUser> userCache = null;

        /// <summary>
        /// Loader data fra en JSON-fil, der indeholder AD-grupper (ad_group_cache).
        /// Formatet er: { "Journalism": ["user0001", ...], "Graphical Designer": ["user0001", ...], ... }
        /// </summary>
        public void LoadCachedGroupData(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"File not found: {filePath}");
                    Console.ResetColor();
                    return;
                }

                string json = File.ReadAllText(filePath);
                // Her parser vi JSON-strukturen direkte til en Dictionary<string,List<string>>.
                groupCache = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Loaded group cache from {filePath}. Groups found: {groupCache?.Count ?? 0}.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error loading group cache from {filePath}: {ex}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Loader data fra en JSON-fil, der indeholder AD-cache (ad_cache).
        /// Formatet er:
        /// {
        ///    "user0824": { "Title":"Anonymous","Department":"Anonymous",...,"Groups":{...} },
        ///    "user0192": { "Title":"Anonymous",...},
        ///    ...
        /// }
        /// </summary>
        public void LoadCachedAdData(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"File not found: {filePath}");
                    Console.ResetColor();
                    return;
                }

                string json = File.ReadAllText(filePath);
                // Bemærk: Vi bruger vores AdUser-model, hvor brugernavne er keys i en dictionary.
                userCache = JsonSerializer.Deserialize<Dictionary<string, ActiveDirectoryUser>>(json);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Loaded AD user cache from {filePath}. Users found: {userCache?.Count ?? 0}.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error loading AD user cache from {filePath}: {ex}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Slår en gruppe op i cached group data (ad_group_cache), 
        /// returnerer en liste af brugernavne fra cachen - i stedet for at kalde AD.
        /// </summary>
        /// <param name="groupName">Navnet på gruppen.</param>
        /// <returns>Liste af userIDs for den gruppe, hvis den findes.</returns>
        public List<string> QueryAdGroupFromCache(string groupName)
        {
            if (groupCache == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning: groupCache is null. Did you call LoadCachedGroupData?");
                Console.ResetColor();
                return new List<string>();
            }

            if (groupCache.TryGetValue(groupName, out var members))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[CACHE] Found {members.Count} members in group '{groupName}'.");
                Console.ResetColor();
                return members;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[CACHE] Group '{groupName}' not found in cache.");
                Console.ResetColor();
                return new List<string>();
            }
        }

        /// <summary>
        /// Samler alle medlemmer fra en liste af grupper - men ved brug af cached data.
        /// </summary>
        /// <param name="groupNames">Liste af gruppenavne.</param>
        /// <returns>Sæt af alle medlemmer fundet i disse grupper.</returns>
        public HashSet<string> CollectAdGroupMembersFromCache(IEnumerable<string> groupNames)
        {
            if (groupCache == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning: groupCache is null. Did you call LoadCachedGroupData?");
                Console.ResetColor();
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            HashSet<string> allMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int groupsFound = 0;
            int groupsNotFound = 0;

            foreach (string group in groupNames)
            {
                if (groupCache.TryGetValue(group, out var members))
                {
                    groupsFound++;
                    foreach (var m in members)
                    {
                        allMembers.Add(m);
                    }
                }
                else
                {
                    groupsNotFound++;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[CACHE] Collected members from {groupsFound} groups. Total unique members: {allMembers.Count}");
            Console.ResetColor();

            if (groupsNotFound > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[CACHE] {groupsNotFound} groups not found in cache.");
                Console.ResetColor();
            }

            return allMembers;
        }

        /// <summary>
        /// Henter en bruger fra cachen ud fra userID (f.eks. "user0192").
        /// </summary>
        /// <param name="userId">UserID der ønskes slået op.</param>
        /// <returns>Et AdUser-objekt hvis det findes i cachen, ellers null.</returns>
        public ActiveDirectoryUser GetAdUserFromCache(string userId)
        {
            if (userCache == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning: userCache is null. Did you call LoadCachedAdData?");
                Console.ResetColor();
                return null;
            }

            if (userCache.TryGetValue(userId, out var adUser))
            {
                return adUser;
            }
            return null;
        }

        // --------------------------------------------------------------------------------
        // Herunder dine eksisterende metoder, der reelt kalder AD via PowerShell.
        // (uændret)
        // --------------------------------------------------------------------------------

        /// <summary>
        /// Queries Active Directory for group members using PowerShell.
        /// </summary>
        public List<string> QueryAdGroup(string groupName)
        {
            try
            {
                string cmd = $@"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8;
Get-ADGroupMember -Identity ""{groupName}"" -Recursive | Select-Object -ExpandProperty SamAccountName
";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-NoProfile -Command \"" + cmd + "\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };

                using (Process proc = new Process { StartInfo = psi })
                {
                    proc.Start();
                    string stdout = proc.StandardOutput.ReadToEnd();
                    string stderr = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(stdout))
                    {
                        var lines = stdout.Trim().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        var groupMembers = lines.ToList();

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Fetched {groupMembers.Count} members from AD group: '{groupName}'.");
                        Console.ResetColor();

                        return groupMembers;
                    }
                    else
                    {
                        string errorMsg = stderr.Trim();
                        if (errorMsg.Contains("Cannot find an object with identity", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"AD group '{groupName}' not found in AD.");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Failed to get AD group details for '{groupName}'. Error: {errorMsg}");
                            Console.ResetColor();
                        }

                        return new List<string>();
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error querying AD group '{groupName}': {e}");
                Console.ResetColor();
                return new List<string>();
            }
        }

        /// <summary>
        /// Collects members from a list of AD groups using live AD queries.
        /// </summary>
        public HashSet<string> CollectAdGroupMembers(IEnumerable<string> groupNames)
        {
            HashSet<string> allMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int groupsQueried = 0;
            int groupsNotFound = 0;

            foreach (string group in groupNames)
            {
                var members = QueryAdGroup(group);
                if (members.Count > 0)
                {
                    foreach (var m in members)
                    {
                        allMembers.Add(m);
                    }
                    groupsQueried++;
                }
                else
                {
                    groupsNotFound++;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Collected members from {groupsQueried} groups.");
            Console.ResetColor();

            if (groupsNotFound > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{groupsNotFound} groups not found or had errors.");
                Console.ResetColor();
            }

            return allMembers;
        }
    }
}
