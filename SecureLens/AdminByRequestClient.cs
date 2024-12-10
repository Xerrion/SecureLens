namespace DefaultNamespace;

public class AdminByRequestClient
{
    private string BaseUrlAudit
    private string BaseUrlInventory = "https://dc1api.adminbyrequest.com/inventory"
    private string ApiKey
    private Dictionary<string, string> Headers
    private string Startdate 
    private string Enddate
    private string Status
    private string Take
    private boolean WantsScanDetails
    private string Type
    
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
        
    }
    
}