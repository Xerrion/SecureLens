namespace SecureLens.Services;

public interface IActiveDirectoryService
{
    List<ActiveDirectoryUser> GetAllUsersCreatedWithinLastMonth();
}