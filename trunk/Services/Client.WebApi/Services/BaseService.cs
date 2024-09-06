using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace Client.WebApi
{
    public class BaseService
    {
        protected IDbConnection CreateTrvwConnection()
        {
            var conTrvwDbString = (new BaseAppConfiguration()).TrvwDbConString;
            return new SqlConnection(conTrvwDbString);
        }
        protected IDbConnection CreateRPConnection()
        {
            var conRPDbString = (new BaseAppConfiguration()).RPDbConString;
            return new SqlConnection(conRPDbString);
        }
    }

    public class BaseAppConfiguration
    {
        private readonly string _conRPDbString = string.Empty;
        private readonly string _conTrvwDbString = string.Empty;
        public BaseAppConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            var root = configurationBuilder.Build();
            _conRPDbString = root.GetConnectionString("RPDbCon");
            _conTrvwDbString = root.GetConnectionString("TrvwDbCon");
        }
        public string RPDbConString
        {
            get => _conRPDbString;
        }

        public string TrvwDbConString
        {
            get => _conTrvwDbString;
        }
    }
}
