namespace SecureLens.Analysis.Results;

public class ApplicationStatisticsResult
{
    public string Vendor { get; set; }
    public bool Preapproved { get; set; }
    public int TotalCount { get; set; }
    public int TechnologyCount { get; set; }
    public int ElevateTerminalRightsCount { get; set; }
    public int GlobalCount { get; set; }
}