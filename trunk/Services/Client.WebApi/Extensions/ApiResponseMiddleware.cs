using Components;
using DocumentFormat.OpenXml.Office2016.Excel;
using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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

            string clientIpAddress = context.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            Tuple<string, string> userDetails = Tuple.Create("", "");
            
            userDetails = GetUserIdFromToken(context);
            var stopWatch = Stopwatch.StartNew();
            ////// Call the DebugContexLog method to log the HttpContext details

            var request = await FormatRequest(context.Request);

            //if (IsDownloadFile(context))
            //{
            //    _logger.Log(LogLevel.Info, $@"Request " + ", CorrelationId: " + correlationId + ",IP: " + clientIpAddress + ",Path: " + context.Request.Path + ",Request Body:" + request);
            //    await this._next(context);
            //}

            context.Request.Headers.Add("CorrelationId", correlationId);
            context.Request.Headers.Add("IpAddress", clientIpAddress);

            
            //string LogDetail = "correlationid:" + correlationId +
            //       ",ip:" + clientIpAddress +
            //       ",path:" + context.Request.Host + context.Request.Path +
            //       ",requestmethod:" + context.Request.Method +
            //       ",roletype:" + userDetails.Item1 +
            //       ",requesterid:"  +
            //       ",requestbody:" + request +
            //       ",referrer:" + context.Request.Headers["Referer"].ToString() +
            //       ",queryparams:" + context.Request.QueryString.ToString();

            string RespLogDetail = "correlationid:" + correlationId +
                       ",ip:" + clientIpAddress +
                       ",path:" + context.Request.Host + context.Request.Path +
                       ",requestmethod:" + context.Request.Method +
                       ",roletype:" + userDetails.Item1 +
                       ",requesterid:";

            string ErrorLogDetail = "correlationid:" + correlationId +
                        ",roletype:" + userDetails.Item1 +
                        ",requesterid:" +
                        ",path:" + context.Request.Host + context.Request.Path;

            if (IsDownloadFile(context))
            {
                _logger.Log(LogLevel.Info, string.Format("request:correlationid:{0},ip:{1},path:{2},requestmethod:{3},roletype:{4},requesterid:{5},requestbody:{6},referrer:{7},queryparams:{8}",
                                        correlationId, clientIpAddress, context.Request.Host + context.Request.Path.ToString(), context.Request.Method, userDetails.Item1, userDetails.Item2, request, "", ""));
                await this._next(context);
            }

            //Request body
            _logger.Log(LogLevel.Info, string.Format("request:correlationid:{0},ip:{1},path:{2},requestmethod:{3},roletype:{4},requesterid:{5},requestbody:{6},referrer:{7},queryparams:{8}",
                         correlationId, clientIpAddress, context.Request.Host + context.Request.Path.ToString(), context.Request.Method, userDetails.Item1, userDetails.Item2, request, "", ""));

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
                        await HandleSuccessRequestAsync(context, bodyAsText, context.Response.StatusCode, RespLogDetail, stopWatch.ElapsedMilliseconds);
                    }
                    else
                    {
                        bodyAsText = await FormatResponse(bodyStream);

                        if ((context.Response.StatusCode == (int)HttpStatusCode.NotFound || context.Response.StatusCode == (int)HttpStatusCode.BadRequest) && bodyAsText != null)
                            await HandleNotSuccessRequestAsync(context, bodyAsText, context.Response.StatusCode, ErrorLogDetail, stopWatch.ElapsedMilliseconds);
                        else
                            await HandleNotSuccessRequestAsync(context, context.Response.StatusCode, ErrorLogDetail, stopWatch.ElapsedMilliseconds);
                    }

                }
                catch (Exception ex)
                {
                    stopWatch.Stop();
                    await HandleExceptionAsync(context, ex, correlationId, clientIpAddress);
                    bodyStream.Seek(0, SeekOrigin.Begin);
                    await bodyStream.CopyToAsync(originalBodyStream);
                }
            }

        }
        private Tuple<string, string> GetUserIdFromToken(HttpContext context)
        {
            string accessToken = null;

            // Check if the Authorization header is present
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();

                // Check if the header is using the Bearer scheme
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    accessToken = authHeader.Substring("Bearer ".Length).Trim();
                }
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadToken(accessToken) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;

                AES256 aes256 = new AES256();

                // Helper to safely get claim value
                string GetClaim(JwtSecurityToken token, string type) =>
                    token?.Claims.FirstOrDefault(c => c.Type == type)?.Value ?? "";

                string clientcode = GetClaim(jsonToken, "clientcode");
                //string employeeCode = GetClaim(jsonToken, "EmployeeCode");
                //string employeeName = GetClaim(jsonToken, "EmployeeName");
                //string profileName = GetClaim(jsonToken, "ProfileName");

                return System.Tuple.Create("", $"{clientcode}");
            }

            else
                return System.Tuple.Create("", "");
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {

            request.EnableBuffering();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);
            return $"{request.QueryString}{bodyAsText}";
            // return $"{request.Method} {request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
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
