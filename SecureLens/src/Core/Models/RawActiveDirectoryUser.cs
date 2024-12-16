namespace SecureLens.Core.Models;

/// <summary>
/// A small helper class to parse the JSON from "cached_admember_queries.json"
/// that has Groups as a List<string>. 
/// We'll map this "Raw" user to the final ActiveDirectoryUser after parsing.
/// </summary>
internal class RawActiveDirectoryUser
{
    public string Title { get; set; }
    public string Department { get; set; }
    public string DistinguishedName { get; set; }
    public DateTime Created { get; set; }

    // The JSON has "Groups": [...strings...]
    public List<string> Groups { get; set; }
}
