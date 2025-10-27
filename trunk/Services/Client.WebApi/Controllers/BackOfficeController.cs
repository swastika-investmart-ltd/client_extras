using Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Client.WebApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace Client.WebApi.Controllers
{
    [Authorize]   
    [Route("[controller]/[action]")]
    [ApiController]
    public class BackOfficeController : ControllerBase
    {        
        private IBackOfficeService _backOfficeService;
        public BackOfficeController(IBackOfficeService backOfficeService)
        {
            _backOfficeService = backOfficeService;
        }

        [HttpPost()]
        public async Task<IActionResult> GetCalculatedBrokerage([FromBody] CalBrokerageRequest param)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
            }

            if ((param.Exch == "NSE" || param.Exch == "BSE") && (param.OptType != "CASH"))
            {
                ModelState.AddModelError("OptionType", "Invalid option type.");
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
            }
            else if ((param.Exch == "NFO" || param.Exch == "BFO"
               || param.Exch == "CDS" || param.Exch == "BCD"
               || param.Exch == "MCX" || param.Exch == "MCX" || param.Exch == "NCOM") && (param.OptType == "CASH"))
            {
                ModelState.AddModelError("OptionType", "Invalid option type.");
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
            }

            var result = await _backOfficeService.GetCalculatedBrokerage(param);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }
    }
}
