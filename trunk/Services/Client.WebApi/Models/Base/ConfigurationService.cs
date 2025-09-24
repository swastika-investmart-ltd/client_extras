using Microsoft.Extensions.Configuration;

namespace Client.WebApi.Models.Base
{
    public class ConfigurationService
    {
        public string JarvisDbConnectionString { get; }
        public string RPDbConnectionString { get; }
        public string TRvwDbConnectionString { get; }
        public string CapsfoDbConnectionString { get; }

        public ConfigurationService(IConfiguration configuration)
        {
            JarvisDbConnectionString = configuration.GetConnectionString("JarvisDbCon");
            RPDbConnectionString = configuration.GetConnectionString("RPDbCon");
            TRvwDbConnectionString = configuration.GetConnectionString("TrvwDbCon");
            CapsfoDbConnectionString = configuration.GetConnectionString("CapsfoDbCon");
        }
    }
}
