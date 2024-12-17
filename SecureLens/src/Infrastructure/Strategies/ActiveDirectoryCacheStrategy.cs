﻿using SecureLens.Core.Models;
using SecureLens.Infrastructure.Interfaces;
using SecureLens.Infrastructure.Logging;

namespace SecureLens.Infrastructure.Strategies;

public class ActiveDirectoryCacheStrategy : IActiveDirectoryCacheStrategy
{
    private Dictionary<string, List<string>> _groupCache = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, ActiveDirectoryUser> _userCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger _logger;

    public ActiveDirectoryCacheStrategy(ILogger logger)
    {
        _logger = logger;
    }

    public void InitializeGroupCache(Dictionary<string, List<string>> groupCache)
    {
        _groupCache = groupCache;
        _logger.LogInfo("Active Directory group cache initialized.");
    }

    public void InitializeUserCache(Dictionary<string, ActiveDirectoryUser> userCache)
    {
        _userCache = userCache;
        _logger.LogInfo("Active Directory user cache initialized.");
    }

    public List<string> QueryAdGroup(string groupName)
    {
        if (_groupCache.TryGetValue(groupName, out List<string>? members))
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
        var groupsNotFound = 0;

        foreach (var group in groupNames)
        {
            List<string>? members = QueryAdGroup(group);
            if (members.Count == 0) groupsNotFound++;
            foreach (var m in members) allMembers.Add(m);
        }

        if (groupsNotFound > 0) _logger.LogWarning($"[CACHE] {groupsNotFound} groups not found in cache.");
        return allMembers;
    }
}