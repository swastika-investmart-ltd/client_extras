using Microsoft.AspNetCore.Mvc;

namespace Client.WebApi
{
    public class ApiKeyAuthorizeAttribute : ServiceFilterAttribute
    {
        public ApiKeyAuthorizeAttribute() : base(typeof(ApiKeyAuthFilter))
        {
        }
    }
}
