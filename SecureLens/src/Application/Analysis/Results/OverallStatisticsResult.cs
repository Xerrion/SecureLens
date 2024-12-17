namespace SecureLens.Application.Analysis.Results;

public class OverallStatisticsResult
{
    public int TotalUniqueUsers { get; set; }
    public int TotalUniqueWorkstations { get; set; }
    public Dictionary<string, int> MembersPerSetting { get; set; }
    public Dictionary<string, HashSet<string>> DepartmentsUnderSetting { get; set; }
    public Dictionary<string, HashSet<string>> TitlesUnderSetting { get; set; }
    public int UsersWith5Elevations { get; set; }
}