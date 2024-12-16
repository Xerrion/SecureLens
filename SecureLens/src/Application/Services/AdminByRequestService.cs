using SecureLens.Application.Services.Interfaces;
using SecureLens.Core.Models;
using SecureLens.Infrastructure.Interfaces;

namespace SecureLens.Application.Services;

public class AdminByRequestService : IAdminByRequestService
{
    private readonly IAdminByRequestRepository _repository;
    private List<AdminByRequestSetting> _settings;
    
    public AdminByRequestService(IAdminByRequestRepository repository)
    {
        _repository = repository;
        _settings = new List<AdminByRequestSetting>();
    }
    
    public void CreateSetting(string name, List<string> groups)
    {
        // Create a new setting and add it to the Settings list
        var setting = new AdminByRequestSetting(name, groups);
        _settings.Add(setting);
    }
}