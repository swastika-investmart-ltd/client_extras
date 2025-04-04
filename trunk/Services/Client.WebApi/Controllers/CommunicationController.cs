using Components;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Threading.Tasks;

namespace Client.WebApi.Controllers
{
    [ApiKeyAuthorize]
    [Route("[controller]/[action]")]
    [ApiController]
    public class CommunicationController : Controller
    {
        private readonly ILog _logger;
        private ICommunicationService _communicationService;
        public CommunicationController(ICommunicationService communicationService, ILog logger)
        {
            _communicationService = communicationService;
            _logger = logger;
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

        [HttpPost()]
        public async Task<IActionResult> SendWhatsapp_InfoBip(CommunicationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
            }
            var result = await _communicationService.SendWhatsapp_InfoBip(request);
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }
        
        [AllowAnonymous]
        [HttpPost()]
        public async Task<IActionResult> SendWhatsapp_InfoBipResp([FromBody] InfoBipResp resp)
        {
            _logger.Log(LogLevel.Debug, $@"SendWhatsapp_InfoBipResp: " + resp);
             return Ok(); 
        }
    }
}
