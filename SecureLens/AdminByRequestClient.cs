namespace DefaultNamespace;

public class AdminByRequestClient
{
    private static readonly HttpClient client = new HttpClient();
    private readonly string BaseUrlAudit;
    private readonly string BaseUrlInventory = "https://dc1api.adminbyrequest.com/inventory";
    private readonly string ApiKey;
    private readonly Dictionary<string, string> Headers;
    private readonly string StartDate;
    private readonly string EndDate;
    private readonly string Status;
    private readonly string Take;
    private readonly bool WantsScanDetails;
    private readonly string Type;
    
    public AdminByRequestClient(string baseUrl, string apiKey)
    {
        BaseUrl = baseUrl;
        ApiKey = apiKey;
        Startdate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
        Enddate = DateTime.Now.ToString("yyyy-MM-dd");
        Status = "Finished";
        Take = "100";
        WantsScanDetails = true;
    }

    public List<string> fetchInventoryData()
    {
        var re
    }
    
}