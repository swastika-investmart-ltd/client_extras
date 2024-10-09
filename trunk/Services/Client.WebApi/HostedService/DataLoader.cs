using Microsoft.Extensions.Hosting;
using System.Threading;
using System;
using System.Threading.Tasks;
using Components;

namespace Client.WebApi.HostedService
{
    public class DataLoader : IHostedService
    {
        private readonly ILog _logger;
        private readonly IWealthBagService _wealthBagService;        
        private readonly IXApiKeysLoader _xapiKeysLoader;
        public DataLoader(ILog logger, IXApiKeysLoader xapiKeysLoader, IWealthBagService wealthBagService)
        {
            _logger = logger;
            _xapiKeysLoader = xapiKeysLoader;
            _wealthBagService = wealthBagService;    
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Load XApiKeys data
                await _xapiKeysLoader.LoadXApiKeysInfoAsync();

                // Load portfolio data
                await _wealthBagService.SavePortfolioDataInMemory();               

            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, " Error loading data at startup: " + ex.Message.ToString() + " - " + ex.StackTrace.ToString());                
            }
        }
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
