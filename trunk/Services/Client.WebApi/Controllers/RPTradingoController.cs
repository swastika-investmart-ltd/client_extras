using Client.WebApi.Services;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResearchPanel.Entities;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace Client.WebApi.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    [ApiController]
    public class RPTradingoController : ControllerBase
    {
        private IRPTradingoService _rpTradingoService;
        private readonly IConfiguration _config;
        private readonly CacheManager<ScripOrderbySegmentsRes> _cacheManager;
        public RPTradingoController(IRPTradingoService rpTradingoService, IConfiguration config, IMemoryCache memoryCache)
        {
            _rpTradingoService =  rpTradingoService; 
            _config = config;
            _cacheManager = new CacheManager<ScripOrderbySegmentsRes>(memoryCache);
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
        public async Task<IActionResult> ViewRecommendationPercentageV1()
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _rpTradingoService.ViewRecommendationPercentageV1();
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
            List<ScripOrderbySegmentsRes> listData = await _cacheManager.GetOrSetListAsync(_config["TopRecommendation:CacheKey"], GetTopRecommendationListFromDatabase, TimeSpan.FromHours(Convert.ToDouble(_config["TopRecommendation:ExpirationHrTime"])));

            var result = new ResponseBaseModel<ScripOrderbySegmentsRes>()
            {
                Datas = listData,
                TotalRows = listData.Count
            };
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        private async Task<List<ScripOrderbySegmentsRes>> GetTopRecommendationListFromDatabase()
        {
            // Fetch a top recommendation list from the database           
            return await _rpTradingoService.GetTopRecommendationListFromDatabase();
        }
    }
}
