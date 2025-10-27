using Client.WebApi.Models;
using Client.WebApi.Services;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Client.WebApi.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        public IContactsService _contactsService;
        public ContactsController(IContactsService contactsService)
        {
            _contactsService = contactsService;
        }
        [HttpPost()]
        public async Task<IActionResult> Contacts([FromBody] InsertContactReq objInsertContact)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ModelStateExtension.AllErrors(ModelState))));
            }
            var result = await _contactsService.InsertContacts(objInsertContact);
            if (result.ResponseId == 0 && !string.IsNullOrEmpty(result.ResponseMessage))
                return NotFound(new ApiResponse(404, new ApiError(result.ResponseMessage)));
            return Ok(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), result, 200));
        }
    }
}
