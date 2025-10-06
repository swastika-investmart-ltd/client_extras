using Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using Client.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Client.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]/[action]")]
    public class LedgerController : ControllerBase
    {
        ILedgerService _ledgerService;
        IConfiguration _config;

        public LedgerController(ILedgerService ledgerService, IConfiguration config)
        {
            _ledgerService = ledgerService;
            _config = config;

        }

        /// <summary>
        /// Retrieves the list of funds added and withdrawn for a client within the current financial year.
        /// </summary>
        /// <param name="obj">LedgerRequest containing client and category details</param>
        /// <returns>ApiResponse with the list of funds added and withdrawn</returns>
        [HttpPost]
        public async Task<IActionResult> GetFundsAddedAndWithdrawnList([FromBody] LedgerRequest obj)
        {
            // Validate the incoming model
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            // Determine the financial year start based on current date (April as start of financial year)
            int FinStartYear = (DateTime.Now.Month >= 4 ? DateTime.Now.Year : DateTime.Now.Year - 1);

            // Prepare internal request object for the service
            LedgerInternalRequest ParamIntr = new LedgerInternalRequest
            {
                ClientCode = obj.Uid,
                CategoryId = obj.CategoryId,
                SubCategoryId = obj.SubCategoryId,
                FromDate = _ledgerService.GetToDateFromConfig(FinStartYear),
                ToDate = DateTime.Now.ToString(@"dd/MM/yyyy"),
                StartYear = FinStartYear.ToString()
            };

            // Call the service to get the data
            var result = await _ledgerService.GetFundsAddedAndWithdrawnList(ParamIntr);
            // Return the result wrapped in ApiResponse
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        /// <summary>
        /// Retrieves the list of funds utilized for a client within the current financial year.
        /// </summary>
        /// <param name="obj">FULedgerRequest containing client and utilization details</param>
        /// <returns>ApiResponse with the list of funds utilized</returns>
        [HttpPost]
        public async Task<IActionResult> GetFundsUtilisedList([FromBody] FULedgerRequest obj)
        {
            // Validate the incoming model
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            // Determine the financial year start based on current date (April as start of financial year)
            int FinStartYear = (DateTime.Now.Month >= 4 ? DateTime.Now.Year : DateTime.Now.Year - 1);

            // Prepare internal request object for the service
            LedgerInternalRequest ParamIntr = new LedgerInternalRequest
            {
                ClientCode = obj.Uid,
                FundsUtilisedIn = obj.FundsUtilisedIn,
                FundsUtilisedFor = obj.FundsUtilisedFor,
                FromDate = _ledgerService.GetToDateFromConfig(FinStartYear),
                ToDate = DateTime.Now.ToString(@"dd/MM/yyyy"),
                StartYear = FinStartYear.ToString()
            };

            // Call the service to get the data
            var result = await _ledgerService.GetFundsUtilisedList(ParamIntr);
            // Return the result wrapped in ApiResponse
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }
    }
}
