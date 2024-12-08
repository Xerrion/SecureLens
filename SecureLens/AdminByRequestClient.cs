namespace DefaultNamespace;

public class AdminByRequestClient
{
    private string BaseUrlAudit
    private string BaseUrlInventory
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
    }

    public List<string> fetchInventoryData()
    {
        
    }
    
}