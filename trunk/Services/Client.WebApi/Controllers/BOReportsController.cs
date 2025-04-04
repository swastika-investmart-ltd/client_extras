using Entities;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System;
using Client.WebApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace Client.WebApi.Controllers
{
    //[Authorize]
    [AllowAnonymous]
    [Route("[controller]/[action]")]
    [ApiController]
    public class BOReportsController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private IReportsService _reportsService;
        public BOReportsController(IReportsService reportsService, IWebHostEnvironment hostingEnvironment)
        {
            _reportsService =  reportsService;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpPost]
        public async Task<IActionResult> GetAnnualPnlSummary([FromBody] AnnualPnlSummaryReqMdl obj)
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
        public async Task<IActionResult> GetGlobalSummary([FromBody] GlobalSummaryReqMdl obj)
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
        public async Task<IActionResult> GetTradeSummaryReport([FromBody] TradeSummaryReqMdl obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _reportsService.GetTradeSummaryReport(obj);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost]
        public async Task<IActionResult> GetTradeSummaryWebReport([FromBody] TradeSummaryReqMdl obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _reportsService.GetTradeSummaryWebReport(obj);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost]
        public async Task<IActionResult> GetHoldingTradeSummaryReport([FromBody] HoldingTradeSummaryReqMdl obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _reportsService.GetHoldingTradeSummaryReport(obj);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost]
        public async Task<IActionResult> DownLoadAnnualReport([FromBody] DownLoadReportReqMdl obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var filePath = _hostingEnvironment.ContentRootPath;
            var result = await _reportsService.GetDownLoadAnnualReport(obj, filePath);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost]
        public async Task<IActionResult> GetMTFInterestReport([FromBody] MTFInterestReportReqMdl obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            var result = await _reportsService.GetMTFInterestReport(obj);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }
    }
}
