using Client.WebApi.Extensions;
using Components;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client.WebApi.Controllers
{
    //[AllowAnonymous]
    [ApiKeyAuthorize]
    [Route("Communication/[action]")]
    [ApiController]
    public class CommunicationController : Controller
    {
        private ICommunicationService _communicationService; 
        public CommunicationController(ICommunicationService communicationService)
        {
            _communicationService = communicationService; 
        }

        [HttpPost()]
        public async Task<IActionResult> SendWhatsapp(CommunicationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
            }
            var result = await _communicationService.SendWhatsapp(request);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [HttpPost()]
        public async Task<IActionResult> TriggerCallViaTATA(CommunicationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
            }
            var result = await _communicationService.TriggerCallViaTATA(request);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }
    }
}
