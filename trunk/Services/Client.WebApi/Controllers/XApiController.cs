using Client.WebApi.Services;
using Entities;
using Microsoft.AspNetCore.Mvc;
using ResearchPanel.Entities;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;

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
        private readonly IReportsService _reportsService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public XApiController(IRPTradingoService rpTradingoService, IConfiguration config, IMemoryCache memoryCache, IReportsService reportsService, IWebHostEnvironment hostingEnvironment)
        {
            _rpTradingoService = rpTradingoService;
            _config = config;
            _cacheManager = new CacheManager<ScripOrderbySegmentsRes>(memoryCache);
            _reportsService = reportsService;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost()]
        public async Task<IActionResult> TopRecommendationClearCacheAndFetchData(SegmentReq obj)
        {
            string cacheKey = string.Empty;  
            if (string.IsNullOrEmpty(obj.Segment))
                cacheKey =  "New";
            else
                cacheKey = obj.Segment;

            // Use the CacheManager to get or set a list with a 4-hour expiration time
            // Clear the cache and fetch data from the database for Top Recommendation
            await _cacheManager.ClearCacheAndFetchDataAsync(cacheKey, async () => await GetTopRecommendationListFromDatabase(obj.Segment), TimeSpan.FromHours(Convert.ToDouble(_config["TopRecommendation:ExpirationHrTime"])));

            //await _cacheManager.ClearCacheAndFetchDataAsync(_config["TopRecommendation:ShortRecom"], GetShortTermRecomFromDb, TimeSpan.FromHours(Convert.ToDouble(_config["TopRecommendation:ExpirationHrTime"])));
            //await _cacheManager.ClearCacheAndFetchDataAsync(_config["TopRecommendation:LongRecom"], GetLongTermRecomFromDb, TimeSpan.FromHours(Convert.ToDouble(_config["TopRecommendation:ExpirationHrTime"])));

            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), "OK", 200));
        }
        private async Task<List<ScripOrderbySegmentsRes>> GetTopRecommendationListFromDatabase(string strSegment)
        {
            // Fetch a top recommendation list from the database           
            return await _rpTradingoService.GetTopRecommendationListFromDatabase(strSegment);
        }

        [HttpPost]
        public async Task<IActionResult> GetAnnualPnlSummaryForJarvis([FromBody] AnnualPnlSummaryReqMdl obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var supportedYears = new[] { (DateTime.Now.Year - 1).ToString(), DateTime.Now.Year.ToString(), (DateTime.Now.Year + 1).ToString() };
            if (!supportedYears.Contains(obj.FinYear.ToString()))
            {
                ModelState.AddModelError("Year", "Invalid year. Only this year and the previous year is allowed.");
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
            }
            var result = await _reportsService.GetAnnualPnlSummary(obj);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost]
        public async Task<IActionResult> GetGlobalSummaryForJarvis([FromBody] GlobalSummaryReqMdl obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var supportedYears = new[] { (DateTime.Now.Year - 1).ToString(), DateTime.Now.Year.ToString(), (DateTime.Now.Year + 1).ToString() };
            if (!supportedYears.Contains(obj.FinYear.ToString()))
            {
                ModelState.AddModelError("Year", "Invalid year. Only this year and the previous year is allowed.");
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
            }
            var result = await _reportsService.GetGlobalSummary(obj);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }
        [HttpPost]
        public async Task<IActionResult> DownLoadAnnualReportForJarvis([FromBody] DownLoadReportReqMdl obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
            obj.IsEmail =false;
            var filePath = _hostingEnvironment.ContentRootPath;
            var result = await _reportsService.GetDownLoadAnnualReport(obj, filePath);
            if (result.ResponseId == 1)
            {
                string outputFilePath = result.ResponseMessage;
                var memory = new MemoryStream();
                using (var stream = new FileStream(outputFilePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
                string contentType;
                if (!provider.TryGetContentType(outputFilePath, out contentType))
                {
                    contentType = "application/octet-stream";
                }
                memory.Position = 0;
                return File(memory, contentType, outputFilePath);
            }
            else
            {
                return NotFound(result.ResponseMessage);
            }
        }
        [HttpPost]
        public async Task<IActionResult> EmailAnnualReportForJarvis([FromBody] DownLoadReportReqMdl obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var filePath = _hostingEnvironment.ContentRootPath;
            var result = await _reportsService.GetDownLoadAnnualReport(obj, filePath);
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

    }
}
