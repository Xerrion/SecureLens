namespace SecureLens.Services;

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
