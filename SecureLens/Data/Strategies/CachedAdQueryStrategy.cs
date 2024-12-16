using SecureLens.Logging;

namespace SecureLens.Data.Stragies;

public class CachedAdQueryStrategy : IAdQueryStrategy
{
    private readonly Dictionary<string, List<string>> _groupCache;
    private readonly ILogger _logger;

    public CachedAdQueryStrategy(Dictionary<string, List<string>> groupCache, ILogger logger)
    {
        _groupCache = groupCache;
        _logger = logger;
    }

    public List<string> QueryAdGroup(string groupName)
    {
        if (_groupCache == null)
        {
            _logger.LogWarning("Warning: groupCache is null. Did you call LoadGroupDataFromFile?");
            return new List<string>();
        }
        if (_groupCache.TryGetValue(groupName, out var members))
        {
            return members;
        }
        else
        {
            _logger.LogWarning($"[CACHE] Group '{groupName}' not found in cache.");
            return new List<string>();
        }
    }

    public HashSet<string> QueryAdGroupMembers(IEnumerable<string> groupNames)
    {
        var allMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int groupsNotFound = 0;

        foreach (string group in groupNames)
        {
            var members = QueryAdGroup(group);
            if (members.Count == 0) groupsNotFound++;
            foreach (var m in members) allMembers.Add(m);
        }

        if (groupsNotFound > 0)
        {
            _logger.LogWarning($"[CACHE] {groupsNotFound} groups not found in cache.");
        }
        return allMembers;
    }
}
