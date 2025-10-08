using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Client.WebApi.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    [ApiController]
    public class WealthBagController : ControllerBase
    {       
        private readonly IWealthBagService _wealthBagService;      
        public WealthBagController(IWealthBagService wealthBagService)
        {          
            _wealthBagService = wealthBagService;
        }

        [AllowAnonymous]
        [HttpPost]
        [ApiKeyAuthorize]
        public async Task<IActionResult> SavePortfolioDataInMemory()
        {
            bool result = await _wealthBagService.SavePortfolioDataInMemory();
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }
       
        [HttpPost]
        public async Task<IActionResult> GetWealthBagDataByClientId(WBDataByUidReq obj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
            }

            var result = new ResponseBaseModelWb<PortfolioData>();
            result = await _wealthBagService.GetWealthBagDataByClientId(obj);

            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }
    }
}
