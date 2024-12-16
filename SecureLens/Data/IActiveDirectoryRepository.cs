namespace SecureLens;

public interface IActiveDirectoryRepository
{
    void LoadGroupDataFromFile(string filePath);
    void LoadUserDataFromFile(string filePath);
    List<string> QueryAdGroupFromFile(string groupName);
    HashSet<string> QueryAdGroupMembersFromFile(IEnumerable<string> groups);
    List<string> QueryAdGroupLive(string groupName);
    HashSet<string> QueryAdGroupMembersLive(IEnumerable<string> groupNames);
    ActiveDirectoryUser GetAdUserFromFile(string userId);
}