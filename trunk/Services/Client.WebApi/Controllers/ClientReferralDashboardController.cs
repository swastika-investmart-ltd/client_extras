//using Client.Models.WebApi;
//using Components;
//using Client.WebApi.Services;
//using Entities;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using System.Net.Http;
//using System.Threading.Tasks;

namespace Client.WebApi.Controllers
{
    //[Authorize]
    //[Route("clientreferraldashboard/[action]")]
    //[ApiController]
    //public class ClientReferralDashboard : Controller
    //{
    //    private readonly ILog _logger;
    //    private IClientReferralService _clientReferralService;
    //    private IConfiguration _config;
    //    private static readonly HttpClient client = new HttpClient();
    //    public ClientReferralDashboard(IClientReferralService clientReferralService, ILog logger, IConfiguration config)
    //    {
    //        _logger = logger;
    //        _clientReferralService = clientReferralService;
    //        _config = config;
    //    }

    
    //    [HttpPost()]
    //    public async Task<IActionResult> ClientReferralDetailsByClientCode(ClientReferral objClient)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
    //        }
    //        var result = await _clientReferralService.ClientReferralDetailsByClientCodeV1(objClient);
    //        return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
    //    }


    //    [HttpPost()]
    //    public async Task<IActionResult> LeadReferralDetailsByClientCode(LeadReferralRequest objClient)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
    //        }
    //        var result = await _clientReferralService.LeadReferralDetailsByClientCode(objClient);
    //        return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
    //    }

    //}
}
