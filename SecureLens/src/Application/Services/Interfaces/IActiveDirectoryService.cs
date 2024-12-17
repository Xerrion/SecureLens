using SecureLens.Core.Models;

namespace SecureLens.Application.Services.Interfaces;

public interface IActiveDirectoryService
{
    public List<ActiveDirectoryUser> GetAllUsersCreatedWithinLastMonth();
}