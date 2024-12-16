namespace SecureLens.Services;

public interface IAdminByRequestService
{
    void CreateSetting(string name, List<string> groups);
}