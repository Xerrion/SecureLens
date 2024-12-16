using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using SecureLens.Data.Stragies;
using SecureLens.Logging;
using SecureLens.Models;

namespace SecureLens.Data;

public class ActiveDirectoryRepository : BaseRepository, IActiveDirectoryRepository
{
    private readonly IAdQueryStrategy _strategy;
    private Dictionary<string, ActiveDirectoryUser> _userCache;

    public ActiveDirectoryRepository(ILogger logger, IAdQueryStrategy strategy)
        : base(logger)
    {
        _strategy = strategy;
        _userCache = new Dictionary<string, ActiveDirectoryUser>();
    }

    public void LoadGroupDataFromFile(string filePath)
    {
        // Kun relevant hvis strategy er CachedAdQueryStrategy – 
        // hvis Live, kan denne være no-op eller smide en NotSupportedException.
        if (_strategy is CachedAdQueryStrategy cacheStrategy)
        {
            var data = LoadJsonFile<Dictionary<string, List<string>>>(filePath);
            if (data != null)
            {
                // Sæt groupCache inde i strategy
                typeof(CachedAdQueryStrategy)
                   .GetField("_groupCache", BindingFlags.NonPublic | BindingFlags.Instance)?
                   .SetValue(cacheStrategy, data);
            }
        }
    }

    public void LoadUserDataFromFile(string filePath)
    {
        var rawDictionary = LoadJsonFile<Dictionary<string, RawActiveDirectoryUser>>(filePath);
        if (rawDictionary == null)
        {
            Logger.LogError($"Failed to parse AD user cache from {filePath} (null result).");
            return;
        }
        _userCache = new Dictionary<string, ActiveDirectoryUser>(StringComparer.OrdinalIgnoreCase);
        foreach (var (userId, raw) in rawDictionary)
        {
            _userCache[userId] = ConvertRawToActiveDirectoryUser(raw);
        }
    }
    
    public List<string> QueryAdGroup(string groupName) => _strategy.QueryAdGroup(groupName);

    public HashSet<string> QueryAdGroupMembers(IEnumerable<string> groupNames) => _strategy.QueryAdGroupMembers(groupNames);

    public ActiveDirectoryUser GetAdUserFromFile(string userId)
    {
        if (_userCache.TryGetValue(userId, out var adUser)) return adUser;

        Logger.LogWarning($"[CACHE] User '{userId}' not found in AD cache.");
        return null;
    }

    private ActiveDirectoryUser ConvertRawToActiveDirectoryUser(RawActiveDirectoryUser raw)
    {
        // Samme logik som tidligere
        var mappedUser = new ActiveDirectoryUser
        {
            Title = raw.Title,
            Department = raw.Department,
            DistinguishedName = raw.DistinguishedName,
            Created = raw.Created,
            Groups = raw.Groups?.Select(g => new ActiveDirectoryGroup { Name = g }).ToList() 
                ?? new List<ActiveDirectoryGroup>()
        };
        return mappedUser;
    }
}
