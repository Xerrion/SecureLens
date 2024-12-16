namespace SecureLens.Data.Stragies;

public interface IAdQueryStrategy
{
    List<string> QueryAdGroup(string groupName);
    HashSet<string> QueryAdGroupMembers(IEnumerable<string> groupNames);
}
