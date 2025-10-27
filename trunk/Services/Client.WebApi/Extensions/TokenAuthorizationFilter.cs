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

            // Extract token 'type' claim
            var tokenTypeClaim = GetClaimValue(context, "type");
            
            if (!string.IsNullOrEmpty(tokenTypeClaim))
            {
                // Handle specific token types - ASTOLD(Access Token Old),GST (Access Token), AST (Access Token)
                switch (tokenTypeClaim)
                {
                    case "ASTOLD":
                        return; // Skip validation for ASTOLD tokens

                    case "GST":
                        if (!ValidatePreLoginToken(context))
                        {
                            SetUnauthorizedResult(context);
                        }
                        return;

                    case "AST":
                        if (!ValidateInputForController(context))
                        {
                            SetUnauthorizedResult(context);
                        }
                        return;

                    default:
                        SetUnauthorizedResult(context);
                        return;
                }
            }
            else
            {
                var tokenRoleClaim = GetClaimValue(context, System.Security.Claims.ClaimTypes.Role);
                var genSource = GetClaimValue(context, "GenSource");
                if (tokenRoleClaim == "Client" && (genSource.ToUpper() == "MOB" || genSource.ToUpper() == "WEB"))
                    return;
                else
                    SetUnauthorizedResult(context);
            }
        }

        // Sets the result to Unauthorized
        private void SetUnauthorizedResult(AuthorizationFilterContext context)
        {
            context.Result = new UnauthorizedResult();
        }

        // Validates input for GST token
        private bool ValidatePreLoginToken(AuthorizationFilterContext context)
        {
            var isPreLogin = bool.TryParse(GetClaimValue(context, "prelogin"), out var result) && result;
            if (isPreLogin)
            {
                var actionName = context.ActionDescriptor.RouteValues["action"]?.ToLower();

                // Actions that are allowed during pre-login
                if (actionName is "getscriporderbysegments" or "getorderfollowup"
                    or "getscripgeneralinfo" or "webcallrecommendation")
                {
                    return true;
                }
            }
            return false;
        }

        // Validates input for AST token
        private bool ValidateInputForController(AuthorizationFilterContext context)
        {
            // var controllerName = context.ActionDescriptor.RouteValues["controller"].ToLower();
            var actionName = context.ActionDescriptor.RouteValues["action"]?.ToLower();

            // Actions that do not require validation (client code not required)
            if (actionName is "getscripgeneral" or "getscripoffers"
                or "getscriporderfollowup" or "viewrecommendationpercentage" or "getrecommendationpercentage")
            {
                return true;
            }

            // Extract and validate UID from the request body
            var uid = ExtractUidFromRequestBody(context);
            if (string.IsNullOrEmpty(uid)) return false;

            // Compare UID with client code claim
            var clientCodeClaim = GetClaimValue(context, "clientcode");
            return clientCodeClaim == uid;
        }

        // Extracts UID from the request body
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

                    if (string.IsNullOrEmpty(bodyContent)) return string.Empty;

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

        // Extracts a claim value from the user principal
        private string GetClaimValue(AuthorizationFilterContext context, string claimType)
        {
            return context.HttpContext.User.FindFirst(claimType)?.Value;
        }
    }
}
