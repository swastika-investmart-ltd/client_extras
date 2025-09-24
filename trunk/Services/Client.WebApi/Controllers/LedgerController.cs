using Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using Client.WebApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace Client.WebApi.Controllers
{
    [AllowAnonymous]
    //[ApiKeyAuthorize]
    [ApiController]
    [Route("[controller]/[action]")]
    public class LedgerController : ControllerBase
    {
        ILedgerService _ledgerService;
        public LedgerController(ILedgerService ledgerService)
        {
            _ledgerService = ledgerService;
        }
        
        [HttpPost]
        public async Task<IActionResult> GetFundsAddedAndWithdrawnList([FromBody] LedgerRequest obj)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));

            int FinStartYear = (DateTime.Now.Month >= 4 ? DateTime.Now.Year : DateTime.Now.Year - 1);
            //(DateTime.Now.Month >= 4 ? DateTime.Now.Year - 1 : DateTime.Now.Year - 2);

            LedgerInternalRequest ParamIntr = new LedgerInternalRequest
            {
                ClientCode = obj.Uid,
                CategoryId = obj.CategoryId,
                SubCategoryId = obj.SubCategoryId,
                FromDate = "01/04/" + FinStartYear.ToString(),
                ToDate = DateTime.Now.ToString(@"dd/MM/yyyy"),
                StartYear = FinStartYear.ToString()
            };

            //await _ledgerService.GetLedgerData(ParamIntr);
            var result = await _ledgerService.Ledger_GetDPCharges(ParamIntr);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }
    }
}
