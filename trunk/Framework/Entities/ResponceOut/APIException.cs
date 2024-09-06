using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Entities
{
    public class ApiException : System.Exception
    {
        public int StatusCode { get; set; }
        public bool IsModelValidatonError { get; set; }
        public IEnumerable<ValidationError> Errors { get; set; }
        //public string ReferenceErrorCode { get; set; }
        //public string ReferenceDocumentLink { get; set; }
        public ApiException(string message,
                            int statusCode = 500,
                            string errorCode = "",
                            string refLink = "") :
            base(message)
        {
            this.IsModelValidatonError = false;
            this.StatusCode = statusCode;
            //this.ReferenceErrorCode = errorCode;
            //this.ReferenceDocumentLink = refLink;
        }

        public ApiException(IEnumerable<ValidationError> errors, int statusCode = 400)
        {
            this.IsModelValidatonError = true;
            this.StatusCode = statusCode;
            this.Errors = errors;
        }

        public ApiException(System.Exception ex, int statusCode = 500) : base(ex.Message)
        {
            this.IsModelValidatonError = false;
            StatusCode = statusCode;
        }
    }

    public class ApiError
    {
        public string ExceptionMessage { get; set; }
        public string Details { get; set; }
        //public string ReferenceErrorCode { get; set; }
        //public string ReferenceDocumentLink { get; set; }
        public IEnumerable<ValidationError> ValidationErrors { get; set; }

        public ApiError(string message)
        {
            this.ExceptionMessage = message;
        }

        public ApiError(string message, IEnumerable<ValidationError> validationErrors)
        {
            this.ExceptionMessage = message;
            this.ValidationErrors = validationErrors;
        }
    }

    [DataContract]
    public class ApiResponse
    {
        [DataMember]
        public int StatusCode { get; set; }

        [DataMember]//(EmitDefaultValue = false)]
        public string Message { get; set; }

        [DataMember]
        public bool IsError { get; set; }

        [DataMember]//(EmitDefaultValue = false)]
        public object ResponseException { get; set; }

        [DataMember]//(EmitDefaultValue = false)]
        public object Result { get; set; }

        [JsonConstructor]
        public ApiResponse(string message, object result = null, int statusCode = 200)
        {
            this.StatusCode = statusCode;
            this.Message = message;
            this.Result = result;
            this.ResponseException = null;
            this.IsError = false;
        }
        //[JsonConstructor]
        public ApiResponse(int statusCode, ApiError apiError)
        {
            this.StatusCode = statusCode;
            this.Message = "Fail";
            this.Result = null;
            this.ResponseException = apiError;
            this.IsError = true;
        }

       
    }

    [DataContract]
    public class TATAApiResponse
    {
        [DataMember]
        public object transfer { get; set; }
        [JsonConstructor]
        public TATAApiResponse(object result = null)
        {
            this.transfer = result;
        }
        //[JsonConstructor]
        public TATAApiResponse()
        {
            this.transfer = null;
        }
    }
    public enum ResponseMessageEnum
    {
        [Description("Request successful.")]
        Success,
        [Description("Request not found. The specified uri does not exist.")]
        NotFound,
        [Description("Request responded with 'Method Not Allowed'.")]
        MethodNotAllowed,
        [Description("Request no content. The specified uri does not contain any content.")]
        NotContent,
        [Description("Request responded with exceptions.")]
        Exception,
        [Description("Request denied. Unauthorized access.")]
        UnAuthorized,
        [Description("Request responded with validation error(s). Please correct the specified validation errors and try again.")]
        ValidationError,
        [Description("Request cannot be processed. Please contact a support.")]
        Unknown,
        [Description("Unhandled Exception occured. Unable to process the request.")]
        Unhandled
    }
   
    public class ValidationError
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; }

        public ValidationError(string field, string message)
        {
            Field = field != string.Empty ? field : null;
            Message = message;
        }
    }

    public static class ModelStateExtension
    {
        public static IEnumerable<ValidationError> AllErrors(this ModelStateDictionary modelState)
        {
            return modelState.Keys.SelectMany(key => modelState[key].Errors.Select(x => new ValidationError(key, x.ErrorMessage))).ToList();
        }
    }



}
