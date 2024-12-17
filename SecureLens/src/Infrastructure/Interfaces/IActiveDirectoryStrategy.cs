namespace SecureLens.Infrastructure.Interfaces;

public interface IActiveDirectoryStrategy
{
    public List<string> QueryAdGroup(string groupName);
    public HashSet<string> QueryAdGroupMembers(IEnumerable<string> groupNames);
}