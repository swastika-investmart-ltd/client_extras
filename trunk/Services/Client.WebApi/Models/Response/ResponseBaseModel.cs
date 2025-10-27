using System.Collections.Generic;

namespace Client.WebApi
{
    public class ResponseBaseModel
    {
        public long ResponseId { get; set; }
        public string ResponseMessage { get; set; }
    }
    public class ResponseBaseTModel<T>
    {
        public T Data { get; set; }
    }
    public class ResponseBaseModel<T>
    {
        public long TotalRows { get; set; }
        public List<T> Datas { get; set; }
    }
    public class ResponseNotificationTLBaseModel<T, T2>
    {
        public T Data { get; set; }
        public List<T2> Datas { get; set; }
    }
    public class ResponseNotificationBaseModelList<T1, T2>
    {
        public List<T1> Datas1 { get; set; }
        public List<T2> Datas2 { get; set; }
    }
    public class ResponseBaseMTFRepModel<T>
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalRows { get; set; }
        public List<T> Datas { get; set; }
    }
    public class ResponseBaseMXCModel
    {
        public long StatusCode { get; set; }
        public string ResponseMessage { get; set; }
        public bool IsError { get; set; }
        public object ResponseException { get; set; }
        public ResponseBaseMCXResult Result { get; set; }
    }
    public class ResponseBaseMCXResult
    {
        public long TotalRows { get; set; }
        public List<MCXUnderlyingInfoResponse> Data { get; set; }
    }
    public class ResponseBaseRecModel<T>
    {
        public int PositiveCalls { get; set; }
        public int NegativeCalls { get; set; }
        public int TotalRows { get; set; }
        public List<T> Datas { get; set; }
    }


    public class ResponseBaseCallRecModel<T1, T2>
    {
        public T1 GraphData { get; set; }
        public List<T2> Datas { get; set; }
        public int TotalRows { get; set; }       
    }

    public class ResponseBaseMobCallRecModel<T1, T2, T3>
    {
        public T1 GraphData { get; set; }
        public List<T2> ActiveDatas { get; set; }
        public List<T3> ClosedDatas { get; set; }
        public int TotalRows { get; set; }
    }

}
