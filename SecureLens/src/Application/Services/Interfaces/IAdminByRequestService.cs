namespace SecureLens.Application.Services.Interfaces;

public interface IAdminByRequestService
{
    public void CreateSetting(string name, List<string> groups);
}