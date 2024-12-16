using SecureLens.Core.Models;

namespace SecureLens.Infrastructure.Interfaces
{
    public interface IActiveDirectoryCacheStrategy : IActiveDirectoryStrategy
    {
        void InitializeGroupCache(Dictionary<string, List<string>> groupCache);
        void InitializeUserCache(Dictionary<string, ActiveDirectoryUser> userCache);
    }
}