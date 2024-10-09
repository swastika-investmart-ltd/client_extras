using System.Net;

namespace Client.WebApi
{
    public class PostDataResponse
    {
        public string Response { get; set; }
        public HttpStatusCode StatusCode { get; set; }

    }
}
