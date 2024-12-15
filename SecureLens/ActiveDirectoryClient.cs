using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace SecureLens
{
    public class ActiveDirectoryClient
    {
        // groupCache: groupName -> list of userIDs (like "user0824", "user0192", etc)
        private Dictionary<string, List<string>> groupCache = null;

        // userCache: userID -> ActiveDirectoryUser (the "mapped" final object)
        private Dictionary<string, ActiveDirectoryUser> userCache = null;
        
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
                groupCache = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error loading group cache from {filePath}: {ex}");
                Console.ResetColor();
            }
        }
        
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

                // First parse into a dictionary: userID -> RawActiveDirectoryUser
                var rawDictionary = JsonSerializer.Deserialize<Dictionary<string, RawActiveDirectoryUser>>(json);
                if (rawDictionary == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to parse AD user cache from {filePath} (null result).");
                    Console.ResetColor();
                    return;
                }

                // Now map them to the final userCache: userID -> ActiveDirectoryUser
                userCache = new Dictionary<string, ActiveDirectoryUser>(StringComparer.OrdinalIgnoreCase);

                foreach (var kvp in rawDictionary)
                {
                    string userId = kvp.Key;  // e.g. "user0824"
                    RawActiveDirectoryUser raw = kvp.Value;
                    
                    var mappedUser = new ActiveDirectoryUser
                    {
                        Title = raw.Title,
                        Department = raw.Department,
                        DistinguishedName = raw.DistinguishedName,
                        Created = raw.Created,
                        Groups = new List<ActiveDirectoryGroup>()
                    };

                    // For each group name in raw.Groups, create an ActiveDirectoryGroup object
                    if (raw.Groups != null)
                    {
                        foreach (var g in raw.Groups)
                        {
                            mappedUser.Groups.Add(new ActiveDirectoryGroup
                            {
                                Name = g
                            });
                        }
                    }

                    userCache[userId] = mappedUser;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error loading AD user cache from {filePath}: {ex}");
                Console.ResetColor();
            }
        }
        
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

            if (groupsNotFound > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[CACHE] {groupsNotFound} groups not found in cache.");
                Console.ResetColor();
            }

            return allMembers;
        }

        /// <summary>
        /// Returns an ActiveDirectoryUser from our "userCache" dictionary, if it exists.
        /// e.g. "user0192" -> AD user object with Title, Department, Groups, etc.
        /// </summary>
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
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[CACHE] User '{userId}' not found in AD cache.");
                Console.ResetColor();
                return null;
            }
        }

        // --------------------------------------------------------------------------------
        // Below: Live AD query methods using PowerShell.
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

            if (groupsNotFound > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{groupsNotFound} groups not found or had errors.");
                Console.ResetColor();
            }

            return allMembers;
        }
    }

    /// <summary>
    /// A small helper class to parse the JSON from "cached_admember_queries.json"
    /// that has Groups as a List<string>. 
    /// We'll map this "Raw" user to the final ActiveDirectoryUser after parsing.
    /// </summary>
    internal class RawActiveDirectoryUser
    {
        public string Title { get; set; }
        public string Department { get; set; }
        public string DistinguishedName { get; set; }
        public DateTime Created { get; set; }

        // The JSON has "Groups": [...strings...]
        public List<string> Groups { get; set; }
    }
}
