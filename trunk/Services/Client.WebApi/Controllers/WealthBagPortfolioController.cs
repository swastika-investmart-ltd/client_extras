using Client.Models.WebApi;
using Components;
using Client.WebApi.Services;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Client.WebApi.Models.WealthbagPortfolio;
using Client.WebApi.Extensions;


namespace Client.WebApi.Controllers
{ 
    [Route("api/WealthBagPortfolio")]
    [ApiController]
    public class WealthBagPortfolio : Controller 
    {
        private readonly ILog _logger;
        private IWealthBagPortfolioService _wealthBagPortfolioService;
        private readonly IConfiguration _configuration;
        public WealthBagPortfolio(IWealthBagPortfolioService wealthBagPortfolioService, ILog logger, IConfiguration config) 
        {
            _logger = logger;
            _wealthBagPortfolioService = wealthBagPortfolioService;
            _configuration = config;
        } 

        [ApiKeyAuthorize]
        [HttpPost("SavePortfolioDataInMemory")]
        public async Task<IActionResult> SavePortfolioDataInMemory()
        {
            bool result = await _wealthBagPortfolioService.SavePortfolioDataInMemory();
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }

        [Authorize]
        [HttpPost("GetWealthBagDataByClientId")]
        public async Task<IActionResult> GetWealthBagDataByClientId(WbPortfolioParam wbPortfolioParam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
            }

            var result = new ResponseBaseModelWb<PortfolioData>();
            result = await _wealthBagPortfolioService.GetWealthBagDataByClientId(wbPortfolioParam.ClientCode);

            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200)); 
        }
    }
}
