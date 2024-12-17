using SecureLens.Core.Models;
using SecureLens.Infrastructure.Interfaces;
using SecureLens.Infrastructure.Logging;
using SecureLens.Infrastructure.Utilities;

namespace SecureLens.Infrastructure.Data.Repositories;

public class ActiveDirectoryRepository : BaseRepository, IActiveDirectoryRepository
{
    private readonly IActiveDirectoryStrategy _strategy;
    private Dictionary<string, ActiveDirectoryUser> _userCache;

    public ActiveDirectoryRepository(ILogger logger, IActiveDirectoryStrategy strategy)
        : base(logger)
    {
        _strategy = strategy;
        _userCache = new Dictionary<string, ActiveDirectoryUser>(StringComparer.OrdinalIgnoreCase);
    }

    public void LoadGroupDataFromFile(string filePath)
    {
        Dictionary<string, List<string>>? data = JsonHelper.LoadJsonFile<Dictionary<string, List<string>>>(filePath);
        if (data != null && _strategy is IActiveDirectoryCacheStrategy cachedStrategy)
        {
            cachedStrategy.InitializeGroupCache(data);
            Logger.LogInfo($"Loaded group data from {filePath}.");
        }
        else if (_strategy is IActiveDirectoryCacheStrategy)
        {
            Logger.LogError($"Failed to parse AD group cache from {filePath}.");
        }
        else
        {
            Logger.LogWarning("LoadGroupDataFromFile called on non-cached strategy.");
        }
    }

    public void LoadUserDataFromFile(string filePath)
    {
        Dictionary<string, RawActiveDirectoryUser>? rawDictionary =
            JsonHelper.LoadJsonFile<Dictionary<string, RawActiveDirectoryUser>>(filePath);
        if (rawDictionary == null)
        {
            Logger.LogError($"Failed to parse AD user cache from {filePath} (null result).");
            return;
        }

        _userCache = new Dictionary<string, ActiveDirectoryUser>(StringComparer.OrdinalIgnoreCase);
        foreach ((var userId, RawActiveDirectoryUser? raw) in rawDictionary)
            _userCache[userId] = ConvertRawToActiveDirectoryUser(raw);

        if (_strategy is IActiveDirectoryCacheStrategy cachedStrategy)
        {
            cachedStrategy.InitializeUserCache(_userCache);
            Logger.LogInfo($"Loaded user data from {filePath}.");
        }
        else
        {
            Logger.LogWarning("LoadUserDataFromFile called on non-cached strategy.");
        }
    }

    public List<string> QueryAdGroup(string groupName)
    {
        return _strategy.QueryAdGroup(groupName);
    }

    public HashSet<string> QueryAdGroupMembers(IEnumerable<string> groupNames)
    {
        return _strategy.QueryAdGroupMembers(groupNames);
    }

    public ActiveDirectoryUser GetAdUserFromFile(string userId)
    {
        if (_userCache.TryGetValue(userId, out ActiveDirectoryUser? adUser)) return adUser;

        Logger.LogWarning($"[CACHE] User '{userId}' not found in AD cache.");
        return null;
    }

    private ActiveDirectoryUser ConvertRawToActiveDirectoryUser(RawActiveDirectoryUser raw)
    {
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