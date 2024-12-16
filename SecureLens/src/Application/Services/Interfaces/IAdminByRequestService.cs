namespace SecureLens.Application.Services.Interfaces;

public interface IAdminByRequestService
{
    void CreateSetting(string name, List<string> groups);
}