namespace SecureLens.Infrastructure.Interfaces;

public interface IActiveDirectoryStrategy
{
    List<string> QueryAdGroup(string groupName);
    HashSet<string> QueryAdGroupMembers(IEnumerable<string> groupNames);
}
