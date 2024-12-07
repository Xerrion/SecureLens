namespace DefaultNamespace;

public class AdminByRequestClient
{
    private string BaseUrl { get; }
    private string Startdate 
    private string Enddate
    private string Status
    
    public AdminByRequestClient(string baseUrl, string apiKey)
    {
        BaseUrl = baseUrl;
        ApiKey = apiKey;
    }

    public List<string> fetchInventoryData()
    {
        var client = new RestClient(BaseUrl);
        var request = new RestRequest("inventory", Method.GET);
        request.AddHeader("Authorization", "Bearer " + ApiKey);
        var response = client.Execute(request);
        var content = response.Content;
        var inventory = JsonConvert.DeserializeObject<List<string>>(content);
        return inventory;
    }
    
}