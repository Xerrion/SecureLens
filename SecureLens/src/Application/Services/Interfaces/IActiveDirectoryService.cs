using SecureLens.Core.Models;

namespace SecureLens.Application.Services.Interfaces;

public interface IActiveDirectoryService
{
    List<ActiveDirectoryUser> GetAllUsersCreatedWithinLastMonth();
}