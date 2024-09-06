using Components;
using Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using NLog;
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
            // Generate a unique ID for the request
            var correlationId = string.Format("{0}{1}", DateTime.Now.Ticks, System.Threading.Thread.CurrentThread.ManagedThreadId);
            context.Request.Headers.Add("CorrelationId", correlationId);

            string clientIpAddress = context.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            //Request body with IP log
            _logger.Log(LogLevel.Info, $@"IP: " + clientIpAddress + " , Path: " + context.Request.Path + " , CorrelationId: " + correlationId);

            if (IsSwagger(context))
                await this._next(context);
            else if (IsElmah(context))
                await this._next(context);
            else if (IsHub(context))
                await this._next(context);
            else
            {
                var originalBodyStream = context.Response.Body;

                using (var bodyStream = new MemoryStream())
                {
                    try
                    {
                        context.Response.Body = bodyStream;
                        string AppId = context.Request.Headers["AppId"];
                        if (!string.IsNullOrEmpty(AppId))
                        {
                            if (context.Request.ContentLength > 0)
                            {
                                var stream = context.Request.Body;
                                var originalReader = new StreamReader(stream);
                                var originalContent = await originalReader.ReadToEndAsync();

                                AES256 aes256 = new AES256();
                                var dataSource = (aes256.Decrypt(originalContent, _config["AES256:Key"]));
                                var requestData = Encoding.UTF8.GetBytes(dataSource);
                                stream = new MemoryStream(requestData);
                                context.Request.Body = stream;
                                _logger.Log(LogLevel.Info, $@"Request   with " + correlationId + " : " + dataSource);
                            }
                        }
                        else
                        {
                            var request = await FormatRequest(context.Request);
                            _logger.Log(LogLevel.Info, $@"Request   with " + correlationId + " : " + request);
                        }
                        await _next.Invoke(context);
                        context.Response.Body = originalBodyStream;
                        if (context.Response.StatusCode == (int)HttpStatusCode.OK)
                        {
                            var bodyAsText = await FormatResponse(bodyStream);
                            await HandleSuccessRequestAsync(context, bodyAsText, context.Response.StatusCode);
                            //Responce body
                            _logger.Log(LogLevel.Info, $@"Response  with " + correlationId + " : " + bodyAsText);
                        }
                        else
                        {
                            var bodyAsText = await FormatResponse(bodyStream);
                            _logger.Log(LogLevel.Warn, $@"Response  with " + correlationId + " : " + context.Response.StatusCode + " : " + bodyAsText);
                            if ((context.Response.StatusCode == (int)HttpStatusCode.NotFound || context.Response.StatusCode == (int)HttpStatusCode.BadRequest) && bodyAsText != null)
                                await HandleNotSuccessRequestAsync(context, bodyAsText, context.Response.StatusCode);
                            else
                                await HandleNotSuccessRequestAsync(context, context.Response.StatusCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, $@"Exception with " + correlationId + " : Stacktrace : " + ex.StackTrace);
                        await HandleExceptionAsync(context, ex);
                        bodyStream.Seek(0, SeekOrigin.Begin);
                        await bodyStream.CopyToAsync(originalBodyStream);
                    }
                    finally
                    {
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

        private Task HandleExceptionAsync(HttpContext context, System.Exception exception)
        {
            ApiError apiError = null;
            int code = 0;

            if (exception is ApiException)
            {
                var ex = exception as ApiException;
                if (ex.IsModelValidatonError)
                {
                    apiError = new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ex.Errors);
                    //{
                    //    ReferenceErrorCode = ex.ReferenceErrorCode,
                    //    ReferenceDocumentLink = ex.ReferenceDocumentLink,
                    //};

                    _logger.Log(LogLevel.Warn, $"[{ex.StatusCode}]: {ResponseMessageEnum.ValidationError.GetDescription()}", exception);
                }
                else
                {
                    apiError = new ApiError(ex.Message);
                    //{
                    //    ReferenceErrorCode = ex.ReferenceErrorCode,
                    //    ReferenceDocumentLink = ex.ReferenceDocumentLink,
                    //};

                    _logger.Log(LogLevel.Warn, $"[{ex.StatusCode}]: {ResponseMessageEnum.Exception.GetDescription()}", exception);
                }

                code = ex.StatusCode;
                context.Response.StatusCode = code;

            }
            else if (exception is UnauthorizedAccessException)
            {
                apiError = new ApiError(ResponseMessageEnum.UnAuthorized.GetDescription());
                code = (int)HttpStatusCode.Unauthorized;
                context.Response.StatusCode = code;

                _logger.Log(LogLevel.Warn, $"[{code}]: {ResponseMessageEnum.UnAuthorized.GetDescription()}", exception);
            }
            else
            {

                var exceptionMessage = ResponseMessageEnum.Unhandled.GetDescription();
                //#if !DEBUG
                //                var message = exceptionMessage;
                //                string stackTrace = null;
                //#else
                var message = $"{ exceptionMessage } { exception.GetBaseException().Message }";
                //                string stackTrace = exception.StackTrace;
                //#endif

                apiError = new ApiError(message);// { Details = stackTrace };
                code = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusCode = code;

                _logger.Log(LogLevel.Error, $"[{code}]: {exceptionMessage}", exception);
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
        private bool IsSwagger(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/swagger");

        }
        // Elmah setting
        private bool IsElmah(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/elmah");

        }
        private bool IsHub(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/negotiate") || context.Request.Path.StartsWithSegments("/campaignhub");

        }
        private JsonSerializerSettings JSONSettings()
        {
            return new JsonSerializerSettings
            {
                //For CamelCase Changes
                //ContractResolver = new CamelCasePropertyNamesContractResolver(),
                //For PascalCase Changes
                ContractResolver = new DefaultContractResolver(),
                //NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };
        }
        //private JsonSerializerSettings JSONSettings()
        //{
        //    return new JsonSerializerSettings
        //    {
        //        //For PascalCase Changes
        //        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        //        //ContractResolver = new DefaultContractResolver(),
        //        //NullValueHandling = NullValueHandling.Ignore,
        //        Converters = new List<JsonConverter> { new StringEnumConverter() }
        //    };
        //}

        private ApiResponse GetErrorResponse(int code, ApiError apiError)
        {
            return new ApiResponse(code, apiError);
        }
    }

    public static class ApiResponseMiddlewareExtension
    {
        public static IApplicationBuilder UseApiResponseAndExceptionWrapper(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiResponseMiddleware>();
        }
    }
}
