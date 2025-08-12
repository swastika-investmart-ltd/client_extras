using Microsoft.Extensions.Hosting;
using System.Threading;
using System;
using System.Threading.Tasks;
using Components;
using Client.WebApi.Services;
using ResearchPanel.Entities;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace Client.WebApi.HostedService
{
    public class DataLoader : IHostedService
    {
        private readonly ILog _logger;
        private readonly IWealthBagService _wealthBagService;        
        private readonly IXApiKeysLoader _xapiKeysLoader;
        private readonly IRPTradingoService _rpTradingoService;
        private readonly CacheManager<ScripOrderbySegmentsRes> _cacheManager;
        public DataLoader(ILog logger, IXApiKeysLoader xapiKeysLoader, IMemoryCache memoryCache, IWealthBagService wealthBagService, IRPTradingoService rpTradingoService)
        {
            _logger = logger;
            _xapiKeysLoader = xapiKeysLoader;
            _wealthBagService = wealthBagService;
            _rpTradingoService = rpTradingoService;
            _cacheManager = new CacheManager<ScripOrderbySegmentsRes>(memoryCache);
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Load XApiKeys data
                await _xapiKeysLoader.LoadXApiKeysInfoAsync();

                // Load portfolio data
                await _wealthBagService.SavePortfolioDataInMemory();

                // Load recommendations into Memory
                await _rpTradingoService.GetAllSegmentsData();
               
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, " Error loading data at startup: " + ex.Message.ToString() + " - " + ex.StackTrace.ToString());                
            }
        }
        
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
