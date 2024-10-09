using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Client.WebApi
{
    public class ApiKeyAuthFilter : BaseService, IAsyncAuthorizationFilter
    {
        private readonly IXApiKeysLoader _xApiKeysLoaderr;
        public ApiKeyAuthFilter(IXApiKeysLoader xApiKeysLoaderr)
        {
            _xApiKeysLoaderr = xApiKeysLoaderr;
        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            string strApiKey = context.HttpContext.Request.Headers["X-API-Key"];
            string strAPIOwner = context.HttpContext.Request.Headers["APIOwner"];

            // Check Missing headers or Validate the API key
            if (string.IsNullOrEmpty(strApiKey) || string.IsNullOrEmpty(strAPIOwner) || !await IsValidApiKeyAsync(strAPIOwner, strApiKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }

        private async Task<bool> IsValidApiKeyAsync(string strAPIOwner, string strApiKey)
        {
            // Check and populate API key dictionary from the database if necessary
            if (XApiKeyDataStore.Reference.xapikeysDictionary == null || XApiKeyDataStore.Reference.xapikeysDictionary.Count <= 0)
            {
                await _xApiKeysLoaderr.LoadXApiKeysInfoAsync();
            }

            // Validate the API key against the dictionary
            return XApiKeyDataStore.Reference.xapikeysDictionary.TryGetValue(strAPIOwner, out var storedApiKey) && storedApiKey.Equals(strApiKey);
        }
    }
}