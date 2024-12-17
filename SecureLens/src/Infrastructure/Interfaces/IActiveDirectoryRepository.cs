using SecureLens.Core.Models;

namespace SecureLens.Infrastructure.Interfaces;

public interface IActiveDirectoryRepository
{
    public void LoadGroupDataFromFile(string filePath);
    public void LoadUserDataFromFile(string filePath);
    public List<string> QueryAdGroup(string groupName);
    public HashSet<string> QueryAdGroupMembers(IEnumerable<string> groupNames);
    public ActiveDirectoryUser GetAdUserFromFile(string userId);
}