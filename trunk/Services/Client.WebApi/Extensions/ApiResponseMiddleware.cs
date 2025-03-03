using Components;
using Entities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2016.Excel;
using System.Web;

namespace Client.WebApi
{
    public class ApiResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILog _logger;
       
        public ApiResponseMiddleware(RequestDelegate next, ILog logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Generate a unique ID for the request
            var correlationId = string.Format("{0}{1}", DateTime.Now.Ticks, System.Threading.Thread.CurrentThread.ManagedThreadId);
            context.Request.Headers.Add("CorrelationId", correlationId);      

            string clientIpAddress = context.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            var request = await FormatRequest(context.Request);
            if (IsDownloadFile(context))
            {
                _logger.Log(LogLevel.Info, $@"Request " + ", CorrelationId: " + correlationId + ",IP: " + clientIpAddress + ",Path: " + context.Request.Path + ",Request Body:" + request);
                await this._next(context);
                _logger.Log(LogLevel.Info, $@"Response " + ", CorrelationId: " + correlationId + ",IP: " + clientIpAddress + ",Path: " + context.Request.Path + ",Response Body:Pnl Report Download" );

            }
            else
            {
                var originalBodyStream = context.Response.Body;

                using (var bodyStream = new MemoryStream())
                {
                    try
                    {
                        context.Response.Body = bodyStream;

                      
                        _logger.Log(LogLevel.Info, $@"Request " + ", CorrelationId: " + correlationId + ",IP: " + clientIpAddress + ",Path: " + context.Request.Path + ",Request Body:" + request);


                        await _next.Invoke(context);
                        context.Response.Body = originalBodyStream;
                        if (context.Response.StatusCode == (int)HttpStatusCode.OK)
                        {
                            var bodyAsText = await FormatResponse(bodyStream);
                            await HandleSuccessRequestAsync(context, bodyAsText, context.Response.StatusCode);
                            //Responce body 
                            _logger.Log(LogLevel.Info, $@"Response " + ", CorrelationId: " + correlationId + ", StatusCode:" + context.Response.StatusCode + ",IP: " + clientIpAddress + ",Path: " + context.Request.Path + ",Response Body:" + bodyAsText);
                        }
                        else
                        {
                            var bodyAsText = await FormatResponse(bodyStream);
                            _logger.Log(LogLevel.Warn, $@"Response " + ", CorrelationId: " + correlationId + ", StatusCode:" + context.Response.StatusCode + ",IP: " + clientIpAddress + ",Path: " + context.Request.Path + ",Response Body:" + bodyAsText);

                            if ((context.Response.StatusCode == (int)HttpStatusCode.NotFound || context.Response.StatusCode == (int)HttpStatusCode.BadRequest) && bodyAsText != null)
                                await HandleNotSuccessRequestAsync(context, bodyAsText, context.Response.StatusCode);
                            else
                                await HandleNotSuccessRequestAsync(context, context.Response.StatusCode);
                        }

                    }
                    catch (Exception ex)
                    {
                        await HandleExceptionAsync(context, ex, correlationId, clientIpAddress);
                        bodyStream.Seek(0, SeekOrigin.Begin);
                        await bodyStream.CopyToAsync(originalBodyStream);
                    }
                }
            }           
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {

            request.EnableBuffering();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);

            return $"{request.Method} {request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
        }

        private async Task<string> FormatResponse(Stream bodyStream)
        {
            bodyStream.Seek(0, SeekOrigin.Begin);
            var plainBodyText = await new StreamReader(bodyStream).ReadToEndAsync();
            bodyStream.Seek(0, SeekOrigin.Begin);

            return plainBodyText;
        }

        private Task HandleExceptionAsync(HttpContext context, System.Exception exception, string correlationId, string clientIpAddress)
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
                _logger.Log(LogLevel.Warn, $@"CorrelationId: " + correlationId + ", StatusCode:" + ex.StatusCode + ",IP: " + clientIpAddress + ",Path: " + context.Request.Path + ",Warning:" + ResponseMessageEnum.ValidationError.GetDescription(), exception);
            }
            else if (exception is UnauthorizedAccessException)
            {
                apiError = new ApiError(ResponseMessageEnum.UnAuthorized.GetDescription());
                code = (int)HttpStatusCode.Unauthorized;
                context.Response.StatusCode = code;

                _logger.Log(LogLevel.Warn, $@" CorrelationId: " + correlationId + ", StatusCode:" + code + ",IP: " + clientIpAddress + ",Path: " + context.Request.Path + ",Warning:" + ResponseMessageEnum.ValidationError.GetDescription(), exception);
            }
            else
            {

                var exceptionMessage = ResponseMessageEnum.Unhandled.GetDescription();               
                var message = $"{exceptionMessage} {exception.GetBaseException().Message}";              

                apiError = new ApiError(message);
                code = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusCode = code;

                _logger.Log(LogLevel.Error, $@" CorrelationId: " + correlationId + ", StatusCode:" + code + ",IP: " + clientIpAddress + ",Path: " + context.Request.Path + ",Exception:" + exceptionMessage, exception);
            }

            var jsonString = ConvertToJSONString(GetErrorResponse(code, apiError));

            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(jsonString);
        }

        private Task HandleNotSuccessRequestAsync(HttpContext context, int code)
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

            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(jsonString);
        }

        private Task HandleNotSuccessRequestAsync(HttpContext context, object body, int code)
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

            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(jsonString);
        }

        private Task HandleSuccessRequestAsync(HttpContext context, object body, int code)
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
            return context.Request.Path.StartsWithSegments("/XApi/DownLoadAnnualReportForJarvis") ;

        } 
    }
}
