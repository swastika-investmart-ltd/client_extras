using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.WebApi.Extensions
{
    public class ApiKeyAuthorizeAttribute : ServiceFilterAttribute
    {
        public ApiKeyAuthorizeAttribute() : base(typeof(ApiKeyAuthorizationFilter)) 
        {
        }
    }
}
