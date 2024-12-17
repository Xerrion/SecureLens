using SecureLens.Core.Models;

namespace SecureLens.Infrastructure.Interfaces;

public interface IActiveDirectoryCacheStrategy : IActiveDirectoryStrategy
{
    public void InitializeGroupCache(Dictionary<string, List<string>> groupCache);
    public void InitializeUserCache(Dictionary<string, ActiveDirectoryUser> userCache);
}