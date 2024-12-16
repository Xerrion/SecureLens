using Microsoft.Extensions.Configuration;

namespace SecureLens.Core.Models
{
    public class SettingsManager
    {
        private readonly IConfiguration _configuration;

        public SettingsManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<AdminByRequestSetting> InitializeSettings()
        {
            // Bind settings from appsettings.json
            var settings = _configuration.GetSection("AdminByRequestSettings")
                .Get<List<AdminByRequestSetting>>();
            
            return settings ?? new List<AdminByRequestSetting>();
        }
    }
}