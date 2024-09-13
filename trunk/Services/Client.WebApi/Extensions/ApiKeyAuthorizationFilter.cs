using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
namespace Client.WebApi.Extensions
{
    public class ApiKeyAuthorizationFilter : IAuthorizationFilter
    {
        private readonly IApiKeyValidator _apiKeyValidator;

        public ApiKeyAuthorizationFilter(IApiKeyValidator apiKeyValidator)
        {
            _apiKeyValidator = apiKeyValidator;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string apiKey = context.HttpContext.Request.Headers["X-API-Key"];
            string APIKeyOwner = context.HttpContext.Request.Headers["APIOwner"];

            if (!_apiKeyValidator.IsValid(apiKey, APIKeyOwner))
            {
                context.Result = new UnauthorizedResult();
            }
        }



    }
}
