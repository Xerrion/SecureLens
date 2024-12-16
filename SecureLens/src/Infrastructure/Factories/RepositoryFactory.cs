using SecureLens.Infrastructure.Data.Repositories;
using SecureLens.Infrastructure.Interfaces;
using SecureLens.Infrastructure.Logging;
using SecureLens.Infrastructure.Strategies;

namespace SecureLens.Infrastructure.Factories
{
    public static class RepositoryFactory
    {
        public static IActiveDirectoryRepository CreateActiveDirectoryRepository(
            ILogger logger,
            bool useLiveData = false,
            string? groupCacheFilePath = null,
            string? userCacheFilePath = null)
        {
            if (useLiveData)
            {
                var liveStrategy = new ActiveDirectoryStrategy(logger);
                return new ActiveDirectoryRepository(logger, liveStrategy);
            }
            else
            {
                var cachedStrategy = new ActiveDirectoryCacheStrategy(logger);

                var repo = new ActiveDirectoryRepository(logger, cachedStrategy);
                
                // Indlæs cache data
                if (!string.IsNullOrEmpty(groupCacheFilePath))
                {
                    repo.LoadGroupDataFromFile(groupCacheFilePath);
                }

                if (!string.IsNullOrEmpty(userCacheFilePath))
                {
                    repo.LoadUserDataFromFile(userCacheFilePath);
                }

                logger.LogInfo("ActiveDirectoryRepository created in cached mode.");
                return repo;
            }
        }

        public static IAdminByRequestRepository CreateAdminByRequestRepository(
            string apiKey,
            ILogger logger,
            bool useLiveData = false,
            string? cachedInventoryPath = null,
            string? cachedAuditLogsPath = null)
        {
            if (useLiveData)
            {
                var liveStrategy = new AdminByRequestStrategy(logger);
                return new AdminByRequestRepository(apiKey, logger, liveStrategy);
            }
            else
            {
                var cachedStrategy = new AdminByRequestCacheStrategy(logger);

                var repo = new AdminByRequestRepository(apiKey, logger, cachedStrategy);

                // Indlæs cache data
                if (!string.IsNullOrEmpty(cachedInventoryPath))
                {
                    repo.LoadCachedInventoryData(cachedInventoryPath);
                }

                if (!string.IsNullOrEmpty(cachedAuditLogsPath))
                {
                    repo.LoadCachedAuditLogs(cachedAuditLogsPath);
                }

                logger.LogInfo("AdminByRequestRepository created in cached mode.");
                return repo;
            }
        }
    }
}
