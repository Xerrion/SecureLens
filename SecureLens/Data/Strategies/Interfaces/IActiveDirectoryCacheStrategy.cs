using SecureLens.Data.Stragies;

namespace SecureLens.Data.Strategies.Interfaces
{
    public interface IActiveDirectoryCacheStrategy : IActiveDirectoryStrategy
    {
        void InitializeGroupCache(Dictionary<string, List<string>> groupCache);
        void InitializeUserCache(Dictionary<string, ActiveDirectoryUser> userCache);
    }
}