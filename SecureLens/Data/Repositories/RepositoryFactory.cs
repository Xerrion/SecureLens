using SecureLens.Data.Stragies;
using SecureLens.Logging;

namespace SecureLens.Data;

public static class RepositoryFactory
{
    public static IActiveDirectoryRepository CreateActiveDirectoryRepository(
        ILogger logger,
        bool useLiveData = false,
        Dictionary<string, List<string>>? initialGroupCache = null)
    {
        if (useLiveData)
            return new ActiveDirectoryRepository(logger, new LiveAdQueryStrategy(logger));
        else
        {
            initialGroupCache ??= new Dictionary<string, List<string>>();
            return new ActiveDirectoryRepository(
                logger,
                new CachedAdQueryStrategy(initialGroupCache, logger)
            );
        }
    }
}