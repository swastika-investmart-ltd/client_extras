using Components;
using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client.WebApi
{
    public class ApiResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILog _logger;
        private IConfiguration _config;

        public ApiResponseMiddleware(RequestDelegate next, ILog logger, IConfiguration config)
        {
            _next = next;
            _logger = logger;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopWatch = Stopwatch.StartNew();

            // Generate a unique ID for the request
            var correlationId = string.Format("{0}{1}", DateTime.Now.Ticks, System.Threading.Thread.CurrentThread.ManagedThreadId);
            context.Request.Headers.Add("CorrelationId", correlationId);

            string clientIpAddress = context.Request.HttpContext.Connection.RemoteIpAddress.ToString();            
            context.Request.Headers.Add("IpAddress", clientIpAddress);

            string clientcode = GetUserIdFromToken(context);  
            var request = await FormatRequest(context.Request); 

            //Request body
            _logger.Log(LogLevel.Info, string.Format("request:correlationid:{0},ip:{1},path:{2},requestmethod:{3},roletype:{4},requesterid:{5},requestbody:{6},referrer:{7},queryparams:{8}",
                         correlationId, clientIpAddress, context.Request.Host + context.Request.Path.ToString(), context.Request.Method, "", clientcode, request, "", ""));

            var (respLogDetail, errorLogDetail) = GetLogString(clientcode, correlationId, clientIpAddress, context);

            if (IsDownloadFile(context))
            {
                await this._next(context);
                _logger.Log(LogLevel.Info, $@"response:" + respLogDetail + ",responsebody:" + "Pnl Report Download" + ",statuscode:" + context.Response.StatusCode + ",apiresponsetime:" + stopWatch.ElapsedMilliseconds);
            }
            else
            {  
                var originalBodyStream = context.Response.Body;
                using (var bodyStream = new MemoryStream())
                {
                    var bodyAsText = string.Empty;
                    try
                    {
                        context.Response.Body = bodyStream;
                        await _next.Invoke(context);
                        context.Response.Body = originalBodyStream;
                        if (context.Response.StatusCode == (int)HttpStatusCode.OK)
                        {
                            bodyAsText = await FormatResponse(bodyStream);
                            stopWatch.Stop();
                            await HandleSuccessRequestAsync(context, bodyAsText, context.Response.StatusCode, respLogDetail, stopWatch.ElapsedMilliseconds);
                        }
                        else
                        {
                            bodyAsText = await FormatResponse(bodyStream);
                            if ((context.Response.StatusCode == (int)HttpStatusCode.NotFound || context.Response.StatusCode == (int)HttpStatusCode.BadRequest) && bodyAsText != null)
                                await HandleNotSuccessRequestAsync(context, bodyAsText, context.Response.StatusCode, errorLogDetail, stopWatch.ElapsedMilliseconds);
                            else
                                await HandleNotSuccessRequestAsync(context, context.Response.StatusCode, errorLogDetail, stopWatch.ElapsedMilliseconds);
                        }

                    }
                    catch (Exception ex)
                    {
                        stopWatch.Stop();
                        await HandleExceptionAsync(context, ex, errorLogDetail, stopWatch.ElapsedMilliseconds);
                        bodyStream.Seek(0, SeekOrigin.Begin);
                        await bodyStream.CopyToAsync(originalBodyStream);
                    }
                }
            } 
        }
        private string GetUserIdFromToken(HttpContext context)
        {
            var tokenTypeClaim = context.User.Claims.FirstOrDefault(c => c.Type == "type")?.Value;
            if (tokenTypeClaim == "AST")
            {
                string clientcode = context.User.Claims.FirstOrDefault(c => c.Type == "clientcode")?.Value;
                return string.IsNullOrWhiteSpace(clientcode) ? string.Empty : clientcode;
            }
            else if(tokenTypeClaim == "GST")
            {            
                string mobile = context.User.Claims.FirstOrDefault(c => c.Type == "mobile")?.Value;
                return string.IsNullOrWhiteSpace(mobile) ? string.Empty : mobile;
            }
            else
            return string.Empty; //CASE For: ASTOLD
        }

        private (string respLogDetail, string errorLogDetail) GetLogString(string requesterId, string correlationId, string clientIpAddress, HttpContext context)
        {
            string respLogDetail = "correlationid:" + correlationId +
            ",ip:" + clientIpAddress +
            ",path:" + context.Request.Host + context.Request.Path +
                       ",requestmethod:" + context.Request.Method +
                       ",requesterid:" + requesterId +
                       ",roletype:";

            string errorLogDetail = "correlationid:" + correlationId 
                        + ",requesterid:" + requesterId 
                        + ",roletype:" + ",path:" + context.Request.Host + context.Request.Path;

            return (respLogDetail, errorLogDetail);

        }
        private async Task<string> FormatRequest(HttpRequest request)
        {

            request.EnableBuffering();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);
            return $"{request.QueryString}{bodyAsText}";
        }

        private async Task<string> FormatResponse(Stream bodyStream)
        {
            bodyStream.Seek(0, SeekOrigin.Begin);
            var plainBodyText = await new StreamReader(bodyStream).ReadToEndAsync();
            bodyStream.Seek(0, SeekOrigin.Begin);

            return plainBodyText;
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception, string errorLogDetail, long APITime)
        {
            ApiError apiError = null;
            int code = 0;

            if (exception is ApiException)
            {
                var ex = exception as ApiException;
                if (ex.IsModelValidatonError)
                {
                    apiError = new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ex.Errors);
                }
                else
                {
                    apiError = new ApiError(ex.Message);
                }

                code = ex.StatusCode;
                context.Response.StatusCode = code;
            }
            else if (exception is UnauthorizedAccessException)
            {
                apiError = new ApiError(ResponseMessageEnum.UnAuthorized.GetDescription());
                code = (int)HttpStatusCode.Unauthorized;
                context.Response.StatusCode = code;
            }
            else
            {

                var exceptionMessage = ResponseMessageEnum.Unhandled.GetDescription();
                var message = $"{exceptionMessage} {exception.GetBaseException().Message}";

                apiError = new ApiError(message);
                code = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusCode = code;
            }

            var jsonString = ConvertToJSONString(GetErrorResponse(code, apiError));

            _logger.Log(LogLevel.Error, $@"error:" + errorLogDetail + ",message:" + jsonString + ",statuscode:" + code + ",apiresponsetime:" + APITime);

            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(jsonString);
        }

        private Task HandleNotSuccessRequestAsync(HttpContext context, int code, string ErrorLogDetail, long APITime)
        {
            ApiError apiError = null;

            if (code == (int)HttpStatusCode.NotFound)
                apiError = new ApiError(ResponseMessageEnum.NotFound.GetDescription());
            else if (code == (int)HttpStatusCode.NoContent)
                apiError = new ApiError(ResponseMessageEnum.NotContent.GetDescription());
            else if (code == (int)HttpStatusCode.MethodNotAllowed)
                apiError = new ApiError(ResponseMessageEnum.MethodNotAllowed.GetDescription());
            else if (code == (int)HttpStatusCode.Unauthorized)
                apiError = new ApiError(ResponseMessageEnum.UnAuthorized.GetDescription());
            else
                apiError = new ApiError(ResponseMessageEnum.Unknown.GetDescription());

            context.Response.StatusCode = code;
            var jsonString = ConvertToJSONString(GetErrorResponse(code, apiError));
            _logger.Log(LogLevel.Error, $@"error:" + ErrorLogDetail + ",message:" + jsonString + ",statuscode:" + code + ",apiresponsetime:" + APITime);
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(jsonString);
        }

        private Task HandleNotSuccessRequestAsync(HttpContext context, object body, int code, string ErrorLogDetail, long APITime)
        {
            string jsonString = string.Empty;

            var bodyText = !body.ToString().IsValidJson() ? ConvertToJSONString(body) : body.ToString();

            dynamic bodyContent = JsonConvert.DeserializeObject<dynamic>(bodyText);
            Type type = bodyContent?.GetType();

            if (type.Equals(typeof(Newtonsoft.Json.Linq.JObject)))
            {
                ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(bodyText);
                if ((apiResponse.StatusCode != code || apiResponse.Result != null) ||
                    (apiResponse.StatusCode == code && apiResponse.Result == null))
                    jsonString = ConvertToJSONString(apiResponse);
                else
                    jsonString = ConvertToJSONString(code, bodyContent);
            }
            else
            {
                jsonString = ConvertToJSONString(code, bodyContent);
            }
            _logger.Log(LogLevel.Error, $@"error:" + ErrorLogDetail + ",message:" + jsonString + ",statuscode:" + code + ",apiresponsetime:" + APITime);

            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(jsonString);
        }

        private Task HandleSuccessRequestAsync(HttpContext context, object body, int code, string RespLogDetail, long APITime)
        {
            string jsonString = string.Empty;

            var bodyText = !body.ToString().IsValidJson() ? ConvertToJSONString(body) : body.ToString();

            dynamic bodyContent = JsonConvert.DeserializeObject<dynamic>(bodyText);
            Type type = bodyContent?.GetType();

            if (type.Equals(typeof(Newtonsoft.Json.Linq.JObject)))
            {
                ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(bodyText);
                if ((apiResponse.StatusCode != code || apiResponse.Result != null) ||
                    (apiResponse.StatusCode == code && apiResponse.Result == null))
                    jsonString = ConvertToJSONString(apiResponse);
                else
                    jsonString = ConvertToJSONString(code, bodyContent);
            }
            else
            {
                jsonString = ConvertToJSONString(code, bodyContent);
            }
            _logger.Log(LogLevel.Info, $@"response:" + RespLogDetail + ",responsebody:" + jsonString + ",statuscode:" + code + ",apiresponsetime:" + APITime);

            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(jsonString);
        }

        private string ConvertToJSONString(int code, object content)
        {
            return JsonConvert.SerializeObject(new ApiResponse(ResponseMessageEnum.Success.GetDescription(), content, code), JSONSettings());
        }
        private string ConvertToJSONString(ApiResponse apiResponse)
        {
            return JsonConvert.SerializeObject(apiResponse, JSONSettings());
        }
        private string ConvertToJSONString(object rawJSON)
        {
            return JsonConvert.SerializeObject(rawJSON, JSONSettings());
        }

        private JsonSerializerSettings JSONSettings()
        {
            return new JsonSerializerSettings
            {
                //For PascalCase Changes
                ContractResolver = new DefaultContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };
        }

        private ApiResponse GetErrorResponse(int code, ApiError apiError)
        {
            return new ApiResponse(code, apiError);
        }
        private bool IsDownloadFile(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/XApi/DownLoadAnnualReportForJarvis");

        }
    }
}
