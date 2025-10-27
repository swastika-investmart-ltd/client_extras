using Components;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Threading.Tasks;

namespace Client.WebApi.Controllers
{
    [AllowAnonymous]
    [Route("[controller]/[action]")]
    [ApiController]
    public class WebHookController : Controller
    {
        private readonly ILog _logger;
        public WebHookController(ILog logger) {  _logger = logger; }
        
        [HttpPost()]
        public async Task<IActionResult> SendWhatsapp_InfoBipResp([FromBody] InfoBipResp resp)
        {
            _logger.Log(LogLevel.Debug, $@"SendWhatsapp_InfoBipResp: " + resp);
            return Ok();
        }
    }
}
