using SecureLens.Core.Models;

namespace SecureLens.Infrastructure.Interfaces;

public interface IActiveDirectoryRepository
{
    void LoadGroupDataFromFile(string filePath);
    void LoadUserDataFromFile(string filePath);
    List<string> QueryAdGroup(string groupName);
    HashSet<string> QueryAdGroupMembers(IEnumerable<string> groupNames);
    ActiveDirectoryUser GetAdUserFromFile(string userId);
}