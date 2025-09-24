using System.Data.SqlClient;
using System.Data;
using Client.WebApi.Models.Base;

namespace Client.WebApi
{
    public class BaseService
    {
        //define ConfigurationService as a static property in BaseService. This way, it can be set once at the application start, and all derived classes can access it.
        protected static ConfigurationService ConfigurationService { get; private set; }

        public static void SetConfigurationService(ConfigurationService configurationService)
        {
            ConfigurationService = configurationService;
        }

        protected IDbConnection CreateJarvisConnection()
        {
            return new SqlConnection(ConfigurationService.JarvisDbConnectionString);
        }

        protected IDbConnection CreateRPConnection()
        {
            return new SqlConnection(ConfigurationService.RPDbConnectionString);
        }

        protected IDbConnection CreateTrvwConnection()
        {
            return new SqlConnection(ConfigurationService.TRvwDbConnectionString);
        }

        protected IDbConnection CreateCapsfoConnection()
        {
            return new SqlConnection(ConfigurationService.CapsfoDbConnectionString);
        }
    }
}
