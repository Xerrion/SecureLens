namespace SecureLens;

public class AdminByRequestSetting
{
    public string Name { get; set; }
    public List<string> ActiveDirectoryGroups { get; set; }
    
    public AdminByRequestSetting(string Name, List<string> ActiveDirectoryGroups)
    {
        this.Name = Name;
        this.ActiveDirectoryGroups = ActiveDirectoryGroups;
    }
}