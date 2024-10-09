using System.IO;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client.WebApi
{
    public interface IHttpClientPostService
    {
        Task<PostDataResponse> PostData<T>(T model, string baseURL, string addressSuffix, string accessToken);
        Task<string> WebRequestPostAsync(string baseURL, string addresssuffix, string data);
    }
    public class HttpClientPostService : IHttpClientPostService
    {
        private readonly IHttpClientFactory _clientFactory;
        public HttpClientPostService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<PostDataResponse> PostData<T>(T model, string baseURL, string addressSuffix, string accessToken)
        {
            PostDataResponse postDataResponse = new PostDataResponse();
            var client = _clientFactory.CreateClient();
            var requestContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var uri = baseURL + addressSuffix;

            if (!string.IsNullOrEmpty(accessToken))
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            var response = await client.PostAsync(uri, requestContent).ConfigureAwait(false);
            postDataResponse.Response = await response.Content.ReadAsStringAsync();
            postDataResponse.StatusCode = response.StatusCode;
            return postDataResponse;
        }

        public async Task<string> WebRequestPostAsync(string baseURL, string addresssuffix, string data)
        {
            string uri = baseURL + addresssuffix;
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentLength = dataBytes.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";

            //request.KeepAlive = false;
            //request.Timeout = 120000;
            //request.ServicePoint.ConnectionLeaseTimeout = 120000;
            //request.ServicePoint.MaxIdleTime = 120000;

            using (Stream requestBody = request.GetRequestStream())
            {
                await requestBody.WriteAsync(dataBytes, 0, dataBytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream streamResponse = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(streamResponse))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}

