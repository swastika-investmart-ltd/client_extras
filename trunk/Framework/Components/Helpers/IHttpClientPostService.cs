using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using System;
using static System.Net.WebRequestMethods;
using StackExchange.Redis;

namespace Components
{
    public interface IHttpClientPostService
    {
        Task<PostDataResponse> PostData<T>(T model, string baseURL, string addressSuffix, string accessToken, string username);

        Task<PostDataResponse> GetDataPanel(string baseURL, string addressSuffix, string accesskey, string accessToken);
        Task<string> WebRequestPostAsync(string baseURL, string addresssuffix, string data, WebRequestHeaders webRequestHeaders);
        Task<string> WebRequestPostAsyncDeseoLogin(string baseURL, WebRequestHeaders webRequestHeaders);
        Task<string> WebRequestPostAsyncDeseoClients(string baseURL, string addresssuffix, string AuthToken, WebRequestHeaders webRequestHeaders);
        Task<string> WebRequestPostAsyncDeseoInvestment(string baseURL, string addresssuffix, string AuthToken, string ClientId, WebRequestHeaders webRequestHeaders);
        Task<string> WebRequestPostAsyncDeseoClientPortfolio(string AuthToken, string ClientId, string PortfolioId, WebRequestHeaders webRequestHeaders);
        Task<string> WebRequestEkycTokenExpirePostAsync(string baseURL, string token, string email, string addresssuffix, string data);
        Task<string> WebRequestStorePortfolioInMemory(WebRequestHeaders webRequestHeaders);
        Task<string> WebRequestGetSSOAuthToken(WebRequestHeadersSSOToken webRequestHeaders);
        Task<string> WebRequestGetSSOToken(WebRequestHeadersSSOToken webRequestHeaders);
        Task<string> WebRequestGetClientCodeByMobileNo(WebRequestHeadersClientCode webRequestHeaders);
    }
    public class HttpClientPostService : IHttpClientPostService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILog _logger;
        public HttpClientPostService(IHttpClientFactory clientFactory, ILog logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<PostDataResponse> PostData<T>(T model, string baseURL, string addressSuffix, string accessToken, string username)
        {
            PostDataResponse postDataResponse = new PostDataResponse();
            var client = _clientFactory.CreateClient();
            var requestContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var uri = baseURL + addressSuffix;

            if (!string.IsNullOrEmpty(accessToken))
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            if (!string.IsNullOrEmpty(username))
                client.DefaultRequestHeaders.Add("email", username);

            var response = await client.PostAsync(uri, requestContent).ConfigureAwait(false);
            postDataResponse.Response = await response.Content.ReadAsStringAsync();
            postDataResponse.StatusCode = response.StatusCode;
            return postDataResponse;
        }

        public async Task<PostDataResponse> GetDataPanel(string baseURL, string addressSuffix, string accesskey, string accessToken)
        {
            PostDataResponse postDataPanelResponse = new PostDataResponse();

            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri(baseURL);

            if (!string.IsNullOrEmpty(accesskey) && !string.IsNullOrEmpty(accessToken))
            {
                if (accesskey == "Authorization")
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                else
                    client.DefaultRequestHeaders.Add(accesskey, accessToken);
            }

            var response = await client.GetAsync(addressSuffix).ConfigureAwait(false);
            postDataPanelResponse.Response = await response.Content.ReadAsStringAsync();
            postDataPanelResponse.StatusCode = response.StatusCode;
            return postDataPanelResponse;
        }

        public async Task<string> WebRequestPostAsync(string baseURL, string addresssuffix, string data, WebRequestHeaders webRequestHeaders)
        {
            string uri = baseURL + addresssuffix;
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentLength = dataBytes.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.Headers.Add("XAPIKey", webRequestHeaders.XAPIKey);
            request.Headers.Add("APIOwner", webRequestHeaders.APIOwner);

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

        public async Task<string> WebRequestPostAsyncDeseoLogin(string baseURL, WebRequestHeaders webRequestHeaders)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            try
            {
                var client = new HttpClient();
                request = new HttpRequestMessage(HttpMethod.Post, baseURL);
                request.Headers.Add("X-API-KEY", webRequestHeaders.XAPIKey);
                var content = new StringContent("{\r\n    \"email\": \"dealer@swastika.co.in\",\r\n    \"password\": \"Password@123\"\r\n}", null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            { 
                _logger.Log(NLog.LogLevel.Error, "WebRequestPostAsyncDeseoLogin - request: " + request.ToString() + " error: " + ex.Message.ToString() + "-" + ex.StackTrace.ToString());
                return "failed";
            }
        }

        public async Task<string> WebRequestPostAsyncDeseoClients(string baseURL, string addresssuffix, string authToken, WebRequestHeaders webRequestHeaders)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            try
            {
                string url = baseURL + addresssuffix;
                var client = new HttpClient();
                request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", "Bearer " + authToken);
                request.Headers.Add("X-Api-Key", webRequestHeaders.XAPIKey);
                var content = new StringContent("{\r\n    \"SecurityKey\": \"70320f77-38dc-4853-b1ad-7538a26ef543\"\r\n}", null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync(); 
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, "WebRequestPostAsyncDeseoClients - request: " + request.ToString() + " error: " + ex.Message.ToString() + " - " + ex.StackTrace.ToString());
                return ex.Message.ToString();
            }

        }

        public async Task<string> WebRequestPostAsyncDeseoInvestment(string baseURL, string addresssuffix, string authToken, string ClientId, WebRequestHeaders webRequestHeaders)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            try
            {
                string url = baseURL + addresssuffix;
                var client = new HttpClient();
                request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", "Bearer " + authToken);
                request.Headers.Add("X-Api-Key", webRequestHeaders.XAPIKey);
                var content = new StringContent("{\r\n   \"clientid\":\"" + ClientId + "\"\r\n}", null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, "WebRequestPostAsyncDeseoInvestment - request: " + request.ToString() + " error: " + ex.Message.ToString() + " - " + ex.StackTrace.ToString());
                return ex.Message.ToString();
            }
        }

        public async Task<string> WebRequestPostAsyncDeseoClientPortfolio(string authToken, string clientId, string portfolioId, WebRequestHeaders webRequestHeaders)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            try
            {
                string BaseUrl = webRequestHeaders.BaseUrl + clientId + "/investment/portfolio/" + portfolioId + "/holding";
                var client = new HttpClient();
                request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);
                request.Headers.Add("Authorization", "Bearer " + authToken);
                request.Headers.Add("X-Api-Key", webRequestHeaders.XAPIKey);
                var content = new StringContent(webRequestHeaders.SecurityKey, null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, "WebRequestPostAsyncDeseoClientPortfolio - request: " + request.ToString() + " error: " + ex.Message.ToString() + "-" + ex.StackTrace.ToString());
                return ex.Message.ToString(); 
            }
        }

        public async Task<string> WebRequestEkycTokenExpirePostAsync(string baseURL, string token, string email, string userid, string Medium)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, baseURL);
                request.Headers.Add("email", email);
                request.Headers.Add("Authorization", "Bearer " + token);

                var content = new StringContent(
              $"{{\"emailaddress\": \"{email}\", \"userId\": \"{userid}\", \"Medium\": \"{Medium}\", \"IsLogout\": 1}}",
              System.Text.Encoding.UTF8,
              "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return ($"Failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        public async Task<string> WebRequestStorePortfolioInMemory(WebRequestHeaders webRequestHeaders)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            try
            { 
                var client = new HttpClient();
                request = new HttpRequestMessage(HttpMethod.Get, webRequestHeaders.BaseUrl);  
                request.Headers.Add("X-API-Key", webRequestHeaders.XAPIKey);  
                request.Headers.Add("APIOwner", webRequestHeaders.APIOwner);  
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                Console.WriteLine(await response.Content.ReadAsStringAsync()); 
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, "WebRequestStorePortfolioInMemory - request: " + request.ToString() + " Error Message: " + ex.Message.ToString() + " - " + ex.StackTrace.ToString());
                return ex.Message.ToString() + " - " + ex.StackTrace.ToString();
            }
        }

  

        public async Task<string> WebRequestGetSSOAuthToken(WebRequestHeadersSSOToken webRequestHeaders)
        { 
            try
            { 
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, webRequestHeaders.URL); 
                var content = new StringContent(
                              $"{{\"authName\": \"{webRequestHeaders.SSOAuthName}\", \"password\": \"{webRequestHeaders.password}\"\r\n }}",
                              System.Text.Encoding.UTF8,
                              "application/json"); 
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
               _logger.Log(NLog.LogLevel.Error, "WebRequestGetSSOAuthToken - request: " + webRequestHeaders.ToString() + " error: " + ex.Message.ToString() + " - " + ex.StackTrace.ToString());
                return ex.Message.ToString();
            }
        }
         
        public async Task<string> WebRequestGetSSOToken(WebRequestHeadersSSOToken webRequestHeaders)
        { 
            try
            {  
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, webRequestHeaders.URL);
                var content = new StringContent(
                    $"{{\"token\": \"{webRequestHeaders.token}\", \"username\": \"{webRequestHeaders.userName}\"\r\n }}",
                    System.Text.Encoding.UTF8,
                    "application/json"); 
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, "WebRequestGetSSOToken - request: " + webRequestHeaders.ToString() + " error: " + ex.Message.ToString() + " - " + ex.StackTrace.ToString());
                return ex.Message.ToString();
            }
        }

        public async Task<string> WebRequestGetClientCodeByMobileNo(WebRequestHeadersClientCode webRequestHeaders)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            try
            {
                var client = new HttpClient();
                request = new HttpRequestMessage(HttpMethod.Post, webRequestHeaders.BaseUrl);
                request.Headers.Add("X-API-Key", webRequestHeaders.XAPIKey);
                request.Headers.Add("APIOwner", webRequestHeaders.APIOwner);
                var content = new StringContent("{\r\n   \"ClientCode\":\"DT39008\"\r\n}", null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, "WebRequestGetClientCodeByMobileNo - request: " + request.ToString() + " Error Message: " + ex.Message.ToString() + " - " + ex.StackTrace.ToString());
                return ex.Message.ToString() + " - " + ex.StackTrace.ToString();
            }
        }


    }
}
