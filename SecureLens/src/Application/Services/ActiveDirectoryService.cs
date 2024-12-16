using SecureLens.Application.Services.Interfaces;
using SecureLens.Core.Models;
using SecureLens.Infrastructure.Interfaces;

namespace SecureLens.Application.Services;

public class ActiveDirectoryService : IActiveDirectoryService
{
    private readonly IActiveDirectoryRepository _repo;

    public ActiveDirectoryService(IActiveDirectoryRepository repo)
    {
        _repo = repo;
    }

    public List<ActiveDirectoryUser> GetAllUsersCreatedWithinLastMonth()
    {
        throw new NotImplementedException();
    }
}
