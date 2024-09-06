using Components;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System;
using System.Data;
using System.Linq;

namespace Client.WebApi.Extensions
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly SqlConnection _dbConnection;
        private IConfiguration _config;
        private readonly ILog _logger;
        public ApiKeyValidator(SqlConnection dbConnection, IConfiguration config, ILog logger)
        {
            _dbConnection = dbConnection;
            _config = config;
            _logger = logger;
        }

        public bool IsValid(string apiKey, string APIKeyOwner)
        {
            var param = new DynamicParameters();
            param.Add("@ApiKey", apiKey);
            param.Add("@APIKeyOwner", APIKeyOwner);
            var ApiKey = "";
            try
            {
                using (SqlConnection conn = new SqlConnection(this._config.GetConnectionString("sqlconnection")))
                {
                    ApiKey = SqlMapper.Query<string>(conn, "Check_WebFlow_ApiKey", param, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    if (!string.IsNullOrEmpty(ApiKey) && ApiKey != "")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, ex.Message.ToString());
                return false;
            }
        }
    }

    public interface IApiKeyValidator
    {
        bool IsValid(string apiKey, string APIKeyOwner);
    }
}
