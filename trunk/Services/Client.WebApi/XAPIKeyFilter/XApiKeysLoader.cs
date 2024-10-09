using Components;
using Dapper;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Client.WebApi
{
    public interface IXApiKeysLoader
    {
        Task LoadXApiKeysInfoAsync();
    }
    public class XApiKeysLoader : BaseService, IXApiKeysLoader
    {
        private readonly ILog _logger;
        public XApiKeysLoader(ILog log)
        {
            _logger = log;
        }
        public async Task LoadXApiKeysInfoAsync()
        {
            try
            {
                using (var con = CreateJarvisConnection())
                {
                    // Open the connection if it's not already open
                    if (con.State != ConnectionState.Open)
                        con.Open();

                    var result = (await SqlMapper.QueryAsync<GetXApiKeyDataInfo>(con, "Validate_GetXApiKeyData", null, commandType: CommandType.StoredProcedure)).ToList();
                    if (result?.Any() == true)
                    {
                        //Build the Dictionary after loading data                            
                        XApiKeyDataStore.Reference.xapikeysDictionary = result.ToDictionary(item => item.APIKeyOwner, item => item.ApiKey);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, " LoadXApiKeysInfoAsync: " + ex.Message.ToString() + " - " + ex.StackTrace.ToString());
            }
            
        }
    }
}
