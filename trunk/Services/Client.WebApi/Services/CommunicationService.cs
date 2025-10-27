using Client.WebApi;
using Client.WebApi.Services;
using Components;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using Prometheus;
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
        private readonly IConfiguration _config;
        private readonly ILog _logger;

        public CommunicationService(IConfiguration config, ILog logger)
        {
            _config = config;
            _logger = logger;
        }
        //public async Task<bool> SendWhatsapp_PickyAssist(CommunicationRequest request)
        //{
        //    if (request.Uid == "GO11240")
        //       await SendWhatsapp_InfoBip(request);

        //    var httpRequest = (HttpWebRequest)WebRequest.Create("http://pickyassist.com/beta/api/v2/push");
        //    httpRequest.Method = "POST";
        //    httpRequest.Accept = "application/json";
        //    httpRequest.ContentType = "application/json";
        //    using (CommPickyAssistMetrics.SendMessageDuration.NewTimer())
        //    {
        //        try
        //        {
        //            InputData[] senderList = new InputData[1];
        //            senderList[0] = new InputData()
        //            {
        //                number = request.MobileNumber,
        //                message = "Message",
        //                template_message = new List<string>
        //                 {
        //                    request.Uid,
        //                    request.Message,
        //                    request.LTP
        //                 }
        //            };

        //            var data = new Whatsapp();
        //            data.token = _config["PickyAssist:token"];
        //            data.application = _config["PickyAssist:application"];
        //            data.interactive_type = _config["PickyAssist:interactive_type"];
        //            data.template_id = _config["PickyAssist:template_id"];
        //            data.data = senderList;

        //            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
        //            {
        //                streamWriter.Write(JsonConvert.SerializeObject(data));
        //            }

        //            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
        //            _logger.Log(LogLevel.Trace, $@"SendWhatsapp- " + httpResponse);
        //            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        //            {
        //                var result = streamReader.ReadToEnd();
        //            }
        //            CommPickyAssistMetrics.SendMessageSuccess.Inc();
        //        }
        //        catch (System.Exception ex)
        //        {
        //            _logger.Log(LogLevel.Error, $@"SendWhatsapp-Exception: " + ex.ToString());
        //            CommPickyAssistMetrics.SendMessageError.Inc();
        //        }
        //    }
        //    return true;
        //}

        public async Task<bool> TriggerCallViaTATA(CommunicationRequest request)
        {
            using (var client = new HttpClient())
            {
                using (CommTATATeleMetrics.SendTATAMessageDuration.NewTimer())
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
                        CommTATATeleMetrics.SendTATAMessageSuccess.Inc();
                    }
                    catch (System.Exception ex)
                    {
                        _logger.Log(LogLevel.Error, $@"AddLeadInTATA-Exception: " + ex.ToString());
                        CommTATATeleMetrics.SendTATAMessageError.Inc();
                    }
                }
            }
            return true;
        }
        public async Task<bool> SendWhatsapp(CommunicationRequest request)
        {
            using (CommPickyAssistMetrics.SendMessageDuration.NewTimer())
            {
                try
                {
                    HttpClient client = new HttpClient
                    {
                        DefaultRequestHeaders =
                        {
                            { "Authorization", _config["InfoBip:AppKey"] },
                            { "Accept", "application/json" }
                        }
                    };

                    string ApiHost = _config["InfoBip:ApiHost"];
                    var infoBip = new InfoBipWhatsapp
                    {
                        messages = new List<Message>
                        {
                            new Message
                            {
                                from = _config["InfoBip:fromNo"],        // # Verified Infobip WhatsApp sender
                                to = request.MobileNumber,    // # Opted-in recipient number
                                callbackData = request.Uid,   // # Optional: any identifier you want to receive in the callback
                                notifyUrl = _config["InfoBip:notifyUrl"],               // # Optional: for delivery status callbacks
                                content = new Content
                                {
                                    templateName =  _config["InfoBip:templateName"],   // # Template name (case-sensitive)
                                    templateData = new TemplateData
                                    {
                                        body = new Body
                                        {
                                            placeholders = new List<string>
                                            {
                                                request.Uid,      // # Placeholder 1
                                                request.Message.TrimEnd(),  // # Placeholder 2
                                                request.LTP,      // # Placeholder 3
                                                _config["InfoBip:RemindMeDeepLink"]  // # Placeholder 4
                                            }
                                        }
                                    },
                                    language = "en"
                                }
                            }
                        }
                    };

                    var jsonContent = System.Text.Json.JsonSerializer.Serialize(infoBip);
                    using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    using var response = await client.PostAsync(ApiHost + _config["InfoBip:ApiURL"], content);

                    string responseString = await response.Content.ReadAsStringAsync();
                    _logger.Log(LogLevel.Debug, $@"Status Code: " +  (int)response.StatusCode);
                    _logger.Log(LogLevel.Debug, $@"Response: " + responseString);
                    CommPickyAssistMetrics.SendMessageSuccess.Inc();
                }
                catch (System.Exception ex)
                {
                    _logger.Log(LogLevel.Error, $@"SendWhatsapp-Exception: " + ex.ToString());
                    CommPickyAssistMetrics.SendMessageError.Inc();
                }
            }
            return true;
        }
    }
}
