using Components;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Client.WebApi
{
    public interface ICommunicationService
    {
        Task<bool> SendWhatsapp(CommunicationRequest request);
        Task<bool> TriggerCallViaTATA(CommunicationRequest request);
    }
    public class CommunicationService : ICommunicationService
    {
        private IConfiguration _config;
        private readonly ILog _logger;
        public CommunicationService(IConfiguration config, ILog logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendWhatsapp(CommunicationRequest request)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create("http://pickyassist.com/beta/api/v2/push");
            httpRequest.Method = "POST";
            httpRequest.Accept = "application/json";
            httpRequest.ContentType = "application/json";
            try
            {
                InputData[] senderList = new InputData[1];
                senderList[0] = new InputData()
                {
                    number =  request.MobileNumber,
                    message = "Message",
                    template_message = new List<string>
                    {
                       request.ClientId,
                       request.Message,
                       request.LTP
                    }
                };

                var data = new Whatsapp();
                data.token = _config["PickyAssist:token"];
                data.application =  _config["PickyAssist:application"];
                data.interactive_type =  _config["PickyAssist:interactive_type"];
                data.template_id =  _config["PickyAssist:template_id"];
                data.data = senderList;

                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(JsonConvert.SerializeObject(data));
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                _logger.Log(LogLevel.Trace, $@"SendWhatsapp- " + httpResponse);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, $@"SendWhatsapp-Exception: " + ex.ToString());
            }
            return true;
        }

        public async Task<bool> TriggerCallViaTATA(CommunicationRequest request)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var token = _config["TATA:TATAToken"];
                    var LeadId = _config["TATA:LeadId"];
                    string apiUrl = "https://api-smartflo.tatateleservices.com/v1/broadcast/lead/" + LeadId;
                    if (!client.DefaultRequestHeaders.Contains("Authorization"))
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Add("Authorization", token);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    }
                    var content = "{\"field_0\":\"" + request.MobileNumber + "\", \"duplicate_option\": \"clone\"}";
                    var resp = await client.PostAsync(apiUrl, new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    _logger.Log(LogLevel.Trace, $@"AddLeadInTATA- " + resp);
                    var result = await resp.Content.ReadAsStringAsync();
                }
                catch (System.Exception ex)
                {
                    _logger.Log(LogLevel.Error, $@"AddLeadInTATA-Exception: " + ex.ToString());
                }
            }
            return true;
        }

    }
}
