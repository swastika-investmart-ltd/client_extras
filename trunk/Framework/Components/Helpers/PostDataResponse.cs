using System.Net;
using System.Net.Http;

namespace Components
{
    public class PostDataResponse
    {
        public string Response { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
    public class WebRequestHeaders
    {
        public string XAPIKey { get; set; }
        public string APIOwner { get; set; }
        public string BaseUrl { get; set; }
        public string SecurityKey { get; set; }


    }
    public class WebRequestHeadersClientCode
    {
        public string XAPIKey { get; set; }
        public string APIOwner { get; set; }
        public string BaseUrl { get; set; }
        public string MobileNo { get; set; } 
    }
    public class DeseoLoginDataResponse
    {
        public string Response { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public HttpContent ResponseData { get; set; }
    }

    public class TenantType
    {
        public string id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }

    public class Body
    {
        public TenantType tenantType { get; set; }
        public string id { get; set; }
        public string email { get; set; }
        public bool emailVerified { get; set; }
        public long mobile { get; set; }
        public bool mobileVerified { get; set; }
        public object secondaryEmail { get; set; }
        public bool secondaryEmailVerified { get; set; }
        public string status { get; set; }
        public string authToken { get; set; }
        public object tenantBank { get; set; }
        public object tenantDetail { get; set; }
        public object tenantInternetProfile { get; set; }
        public object tenantKRACredentials { get; set; }
    }

    public class ResponseDeseoLogin 
    {
        public int status { get; set; }
        public string message { get; set; }
        public Body body { get; set; }
    }

    public class WebRequestHeadersSSOToken
    {
        public string URL { get; set; }
        public string SSOAuthName { get; set; }
        public string password { get; set; } 
        public string token { get; set; } 
        public string userName { get; set; }
    }


}
