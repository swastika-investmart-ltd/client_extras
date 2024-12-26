using Client.WebApi.Services;
using Entities;
using Microsoft.AspNetCore.Mvc;
using ResearchPanel.Entities;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace Client.WebApi.Controllers
{
    [ApiKeyAuthorize]
    [Route("[controller]/[action]")]
    [ApiController]
    public class XApiController : ControllerBase
    {
        private readonly IRPTradingoService _rpTradingoService;
        private readonly IConfiguration _config;
        private readonly CacheManager<ScripOrderbySegmentsRes> _cacheManager;
        public XApiController(IRPTradingoService rpTradingoService, IConfiguration config, IMemoryCache memoryCache)
        {
            _rpTradingoService = rpTradingoService;
            _config = config;
            _cacheManager = new CacheManager<ScripOrderbySegmentsRes>(memoryCache);
        }

        [HttpPost()]
        public async Task<IActionResult> TopRecommendationClearCacheAndFetchData([FromBody] TopRecommLstReq obj)
        {
            // Clear the cache and fetch data from the database for Top Recommendation
            await _cacheManager.ClearCacheAndFetchDataAsync(_config["TopRecommendation:CacheKey"], GetTopRecommendationListFromDatabase, TimeSpan.FromHours(Convert.ToDouble(_config["TopRecommendation:ExpirationHrTime"])));                        
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), "OK", 200));
        }

        private async Task<List<ScripOrderbySegmentsRes>> GetTopRecommendationListFromDatabase()
        {
            // Fetch a top recommendation list from the database           
            return await _rpTradingoService.GetTopRecommendationListFromDatabase();
        }
    }
}
