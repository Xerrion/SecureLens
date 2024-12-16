using System.Diagnostics;
using System.Text;
using System.Text.Json;
using SecureLens.Logging;
using SecureLens.Models;

namespace SecureLens.Data;

public class ActiveDirectoryRepository : IActiveDirectoryRepository
    {
        private Dictionary<string, List<string>> GroupCache;
        private Dictionary<string, ActiveDirectoryUser> UserCache;

        private readonly ILogger _logger;

        public ActiveDirectoryRepository(ILogger logger)
        {
            _logger = logger;
            GroupCache = new Dictionary<string, List<string>>();
            UserCache = new Dictionary<string, ActiveDirectoryUser>();
        }

        public void LoadGroupDataFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError($"File not found: {filePath}");
                    return;
                }

                string json = File.ReadAllText(filePath);
                GroupCache = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading group cache from {filePath}: {ex}");
            }
        }

        public void LoadUserDataFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError($"File not found: {filePath}");
                    return;
                }

                string json = File.ReadAllText(filePath);

                var rawDictionary = JsonSerializer.Deserialize<Dictionary<string, RawActiveDirectoryUser>>(json);
                if (rawDictionary == null)
                {
                    _logger.LogError($"Failed to parse AD user cache from {filePath} (null result).");
                    return;
                }

                UserCache = new Dictionary<string, ActiveDirectoryUser>(StringComparer.OrdinalIgnoreCase);

                foreach (var kvp in rawDictionary)
                {
                    string userId = kvp.Key;
                    RawActiveDirectoryUser raw = kvp.Value;

                    var mappedUser = new ActiveDirectoryUser
                    {
                        Title = raw.Title,
                        Department = raw.Department,
                        DistinguishedName = raw.DistinguishedName,
                        Created = raw.Created,
                        Groups = new List<ActiveDirectoryGroup>()
                    };

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

                    UserCache[userId] = mappedUser;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading AD user cache from {filePath}: {ex}");
            }
        }

        public List<string> QueryAdGroupFromFile(string groupName)
        {
            if (GroupCache == null)
            {
                _logger.LogWarning("Warning: groupCache is null. Did you call LoadCachedGroupData?");
                return new List<string>();
            }

            if (GroupCache.TryGetValue(groupName, out var members))
            {
                return members;
            }
            else
            {
                _logger.LogWarning($"[CACHE] Group '{groupName}' not found in cache.");
                return new List<string>();
            }
        }

        public HashSet<string> QueryAdGroupMembersFromFile(IEnumerable<string> groups)
        {
            if (GroupCache == null)
            {
                _logger.LogWarning("Warning: groupCache is null. Did you call LoadCachedGroupData?");
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            HashSet<string> allMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int groupsFound = 0;
            int groupsNotFound = 0;

            foreach (string group in groups)
            {
                if (GroupCache.TryGetValue(group, out var members))
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
                _logger.LogWarning($"[CACHE] {groupsNotFound} groups not found in cache.");
            }

            return allMembers;
        }

        public ActiveDirectoryUser GetAdUserFromFile(string userId)
        {
            if (UserCache == null)
            {
                _logger.LogWarning("Warning: userCache is null. Did you call LoadCachedAdData?");
                return null;
            }

            if (UserCache.TryGetValue(userId, out var adUser))
            {
                return adUser;
            }
            else
            {
                _logger.LogWarning($"[CACHE] User '{userId}' not found in AD cache.");
                return null;
            }
        }

        public List<string> QueryAdGroupLive(string groupName)
        {
            try
            {
                var cmd = $@"
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

                using (Process proc = new Process())
                {
                    proc.StartInfo = psi;
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
                            _logger.LogWarning($"AD group '{groupName}' not found in AD.");
                        }
                        else
                        {
                            _logger.LogError($"Failed to get AD group details for '{groupName}'. Error: {errorMsg}");
                        }

                        return new List<string>();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error querying AD group '{groupName}': {e}");
                return new List<string>();
            }
        }

        public HashSet<string> QueryAdGroupMembersLive(IEnumerable<string> groupNames)
        {
            HashSet<string> allMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int groupsQueried = 0;
            int groupsNotFound = 0;

            foreach (string group in groupNames)
            {
                var members = QueryAdGroupLive(group);
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
                _logger.LogWarning($"{groupsNotFound} groups not found or had errors.");
            }

            return allMembers;
        }
    }