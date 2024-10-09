using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Client.WebApi.Extensions
{
    public class TokenAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if the action has the AllowAnonymous attribute or ApiKeyAuthorize attribute, bypass the filter if so          
            if (context.ActionDescriptor.EndpointMetadata.Any(em => em is AllowAnonymousAttribute || em is ApiKeyAuthorizeAttribute))
            {
                return; // Skip authorization for anonymous endpoints or XApi endpoints
            }

            // Check if the user is authenticated
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Extract the token 'type' claim from the token
            var tokenTypeClaim = context.HttpContext.User.FindFirst("type")?.Value;

            //If ASTOLD (Access Token Old)
            if (tokenTypeClaim == "ASTOLD")
            {
                return;
            }

            // Default to requiring AST (Access Token)
            if (tokenTypeClaim == "AST")
            {
                // Perform input validation based on controller logic
                if (!ValidateInputForController(context))
                {
                    //context.Result = new BadRequestObjectResult(new ApiResponse(400, new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), new List<ValidationError> { new ValidationError("Uid", "Uid is invalid.") })));
                    context.Result = new UnauthorizedResult();
                    return;
                }

                return; // Proceed if the token type is correct and input validation passes
            }
            else
            {
                // Return Unauthorized if the token type doesn't match
                context.Result = new UnauthorizedResult();
                return;
            }
        }

        private bool ValidateInputForController(AuthorizationFilterContext context)
        {
            //var controllerName = context.ActionDescriptor.RouteValues["controller"].ToLower();
            var actionName = context.ActionDescriptor.RouteValues["action"].ToLower();
          
            if (actionName.Equals("getscripgeneral")
                || actionName.Equals("getscripoffers")
                || actionName.Equals("getscriporderfollowup")
                || actionName.Equals("viewrecommendationpPercentage"))
            {
                return true; // No validation needed, proceed
            }            

            // Extract and validate the required parameter from the body
            var Uid = ExtractUidFromRequestBody(context);
            if (string.IsNullOrEmpty(Uid))
            {
                return false; // Fail if Uid is empty
            }

            // Extract the token 'type' claim from the token
            var clientcodeClaim = context.HttpContext.User.FindFirst("clientcode")?.Value;
            return clientcodeClaim == Uid;
        }        

        private string ExtractUidFromRequestBody(AuthorizationFilterContext context)
        {
            // Enable buffering to allow multiple reads of the request body
            context.HttpContext.Request.EnableBuffering();

            try
            {
                using (var reader = new StreamReader(context.HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    var bodyContent = reader.ReadToEndAsync().Result;
                    context.HttpContext.Request.Body.Position = 0; // Reset the stream position                    

                    if (string.IsNullOrEmpty(bodyContent))
                        return string.Empty;

                    // Support for both objects and arrays - Parse the body and extract 'Uid' if present
                    var jsonToken = JToken.Parse(bodyContent);
                    var uid = jsonToken.Type switch
                    {
                        JTokenType.Array => jsonToken.FirstOrDefault()?["Uid"],
                        JTokenType.Object => jsonToken["Uid"],
                        _ => null
                    };
                    return uid?.ToString() ?? string.Empty;                  
                }
            }
            catch (JsonReaderException)
            {
                return string.Empty; // Return Empty for invalid JSON
            }
            finally
            {
                // Ensure the request body stream is reset even in case of exception
                context.HttpContext.Request.Body.Position = 0;
            }
        }
    }
}
