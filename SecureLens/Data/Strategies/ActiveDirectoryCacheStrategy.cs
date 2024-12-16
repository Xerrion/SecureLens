using SecureLens.Data.Strategies.Interfaces;
using SecureLens.Logging;

namespace SecureLens.Data.Strategies
{
    public class ActiveDirectoryCacheStrategy : IActiveDirectoryCacheStrategy
    {
        private Dictionary<string, List<string>> _groupCache = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, ActiveDirectoryUser> _userCache = new Dictionary<string, ActiveDirectoryUser>(StringComparer.OrdinalIgnoreCase);
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
}
