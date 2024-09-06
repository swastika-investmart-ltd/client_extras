using System.Collections.Generic;

namespace Entities
{
    public class ResponseBaseModel
    {
        public long ResponseId { get; set; } 
        public string ResponseMessage { get; set; }
    }         

    public class ResponseBaseSModel<T>
    {
        public T Data { get; set; }
    }

    public class ResponseBaseLModel<T>
    {
        public long TotalRows { get; set; }
        public List<T> Data { get; set; }
    }

    public class ResponseBaseSWMModel<T>
    {
        public long ResponseId { get; set; }
        public string ResponseMessage { get; set; }
        public T Data { get; set; }
    }

    public class ResponseBaseLWMModel<T>
    {
        public long ResponseId { get; set; }
        public string ResponseMessage { get; set; }
        public List<T> Data { get; set; }
    }

    public class ResponseBaseTPSSOModel
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }

}
