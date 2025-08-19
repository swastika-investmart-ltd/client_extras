using Client.WebApi.Services;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ResearchPanel.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace Client.WebApi.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    [ApiController]
    public class RPTradingoController : ControllerBase
    {
        private readonly IRPTradingoService _rpTradingoService;
        private readonly IReportsService _reportsService;
        private readonly IConfiguration _config;
        private readonly CacheManager<ScripOrderbySegmentsRes> _cacheManager;
        public RPTradingoController(IRPTradingoService rpTradingoService, IConfiguration config, IMemoryCache memoryCache, IReportsService reportsService)
        {
            _rpTradingoService =  rpTradingoService;
            _config = config;
            _cacheManager = new CacheManager<ScripOrderbySegmentsRes>(memoryCache);
            _reportsService = reportsService;
        }

        [HttpPost] //Note: Remove this after change - GetScripGeneralInfo
        public async Task<IActionResult> GetScripGeneral([FromBody] GSGeneralReq obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _rpTradingoService.GetScripGeneral(obj.CompanyId);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost]
        public async Task<IActionResult> GetScripGeneralInfo([FromBody] GSGeneralInfoReq obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _rpTradingoService.GetScripGeneral(obj.CompanyId);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost] //Note: Remove this after change - GetScripOffersInfo
        public async Task<IActionResult> GetScripOffers([FromBody] GSOffersReq obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _rpTradingoService.GetScripOffers(obj.CompanyId);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost]
        public async Task<IActionResult> GetScripOffersInfo([FromBody] GSOffersInfoReq obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _rpTradingoService.GetScripOffers(obj.CompanyId);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost()] //Note: Remove this after change - GetOrderFollowup
        public async Task<IActionResult> GetScripOrderFollowup([FromBody] GSOrderFollowupReq obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _rpTradingoService.GetScripOrderFollowup(obj.OrderId);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost()]
        public async Task<IActionResult> GetOrderFollowup([FromBody] GOrderFollowupReq obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _rpTradingoService.GetScripOrderFollowup(obj.OrderId);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost()]
        public async Task<IActionResult> GetAllScripInfo([FromBody] GAllScripInfoReq obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _rpTradingoService.GetAllScripInfo(obj.UId, obj.CompanyId);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost()]
        public async Task<IActionResult> GetAllScripInfoWithPagination([FromBody] GAllScripInfoPaginationReq obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _rpTradingoService.GetAllScripInfoWithPagination(obj.Uid, obj.PageNo, obj.CompanyId);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost]
        public async Task<IActionResult> GetScripOrderbySegments([FromBody] ScripOrderbySegmentsReq obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _rpTradingoService.GetScripOrderbySegments(obj);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost()]
        public async Task<IActionResult> GetRecommendationPercentage()
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _rpTradingoService.GetRecommendationPercentage();
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost()] //Note: Remove this after change - ViewRecommendation
        public async Task<IActionResult> ViewRecommendationPercentage()
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _rpTradingoService.ViewRecommendationPercentage();
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost()]
        public async Task<IActionResult> ViewRecommendation(ViewRecomReq obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _rpTradingoService.ViewRecommendationPercentage();
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }
         
        [HttpPost()]
        public async Task<IActionResult> GetTopRecommendationList([FromBody] TopRecommLstReq obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            // Use the CacheManager to get or set a list with a 4-hour expiration time
            string cacheKey = string.Empty;
            if (string.IsNullOrWhiteSpace(obj.Preferred_Segment))
                cacheKey =  "Commodity_Delivery_FNO_Intraday";
            else
            {
               // cacheKey = obj.Preferred_Segment.Trim().Replace(",", "_"); 
                // Step 1: Split by comma
                var parts = obj.Preferred_Segment
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())             // Trim spaces
                    .OrderBy(x => x)                   // Sort alphabetically
                    .ToList();

                // Step 2: Join back with underscore
                cacheKey = string.Join("_", parts);
            }
            //if (cacheKey == "Commodity_Delivery_FNO_Intraday")
            //    cacheKey =  "New";

            List<ScripOrderbySegmentsRes> listData = _cacheManager.Get<List<ScripOrderbySegmentsRes>>(cacheKey.Trim());
            var result = new ResponseBaseModel<ScripOrderbySegmentsRes>()
            {
                Datas = listData?.ToList() ?? new List<ScripOrderbySegmentsRes>(),
                TotalRows = listData?.ToList().Count ?? 0
            };
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        } 

        [HttpPost()]
        public async Task<IActionResult> GetRecommendations([FromBody] TopRecommLstReq obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            List<ScripOrderbySegmentsRes> listData = new();
            // Use the CacheManager to get or set a list with a 4-hour expiration time
            if (obj.IsShortTerm == true)
                listData = await _cacheManager.GetOrSetListAsync(_config["TopRecommendation:ShortRecom"], GetShortTermRecomFromDb, TimeSpan.FromHours(Convert.ToDouble(_config["TopRecommendation:ExpirationHrTime"])));
            else
                listData = await _cacheManager.GetOrSetListAsync(_config["TopRecommendation:LongRecom"], GetLongTermRecomFromDb, TimeSpan.FromHours(Convert.ToDouble(_config["TopRecommendation:ExpirationHrTime"])));

            var result = new ResponseBaseModel<ScripOrderbySegmentsRes>()
            {
                Datas = listData,
                TotalRows = listData.Count
            };
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }
        private async Task<List<ScripOrderbySegmentsRes>> GetShortTermRecomFromDb()
        {
            //// Call this api also to update the cache
            return await _rpTradingoService.GetShortTermRecomFromDb();
        }
        private async Task<List<ScripOrderbySegmentsRes>> GetLongTermRecomFromDb()
        {
            //// Call this api also to update the cache
            return await _rpTradingoService.GetLongTermRecomFromDb();
        }

        [HttpPost]
        public async Task<IActionResult> WebCallRecommendation([FromBody] OrderbySegmentsReq obj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
            }

            var result = await _rpTradingoService.GetWebCallRecommendation(obj);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost]
        public async Task<IActionResult> MobCallRecommendation([FromBody] OrderbySegmentsReq obj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
            }

            var result = await _rpTradingoService.GetMobCallRecommendation(obj);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }
    }
}
