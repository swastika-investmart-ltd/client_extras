using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class ResponseBaseModel
    {
        public long ResponseId { get; set; }
        public string ResponseMessage { get; set; }
    }
    public class ResponseBaseSModel<T>
    {
        public long ResponseId { get; set; }
        public string ResponseMessage { get; set; }
        public T Data { get; set; }
    }

    public class ResponseBaseSSModel<T>
    {
        public long ResponseId { get; set; }
        public string ResponseMessage { get; set; }
        public T Transfer { get; set; }
    }

    

    public class ResponseBaseModelDD
    {
        public long Id { get; set; }
        public string Value { get; set; }
    }

    public class ResponseBaseModel<T>
    {
        public long TotalRows { get; set; }
        public List<T> Datas { get; set; }
    }

    public class ResponseBaseModelLeadList<T>
    {
        public long TotalRows { get; set; }
        public long TotalFilteredRows { get; set; }
        public List<T> Datas { get; set; }
    }

    public class ResponseBaseFileModel<T>
    {
        public long TotalRows { get; set; }
        public List<T> Datas { get; set; }
    }


    public class ResponseBaseFileModel
    {
        public long ResponseId { get; set; }
        public string ResponseMessage { get; set; }
        public string FilePath { get; set; }
    }

    public class ResponseBaseModelLead 
    {
        public long ResponseId { get; set; }
        public long ResponseLeadId { get; set; }
        public string ResponseMessage { get; set; }
    }

    public class ResponseBaseLModel<T>
    {
        public long TotalRows { get; set; }
        public List<T> Data { get; set; }
    }
    public class ResponseBaseModelEMP<T>
    {
        public bool IsSuccess { get; set; }
        public string ResponseMessage { get; set; }
        public List<T> Datas { get; set; }
        public long TotalRows { get; set; }
    }

    public class ResponseBaseModelClient<T1,T2>
    {
        public double TotalReferralAmtEarned { get; set; }
        public long TotalReferredClient { get; set; }
        public long ReferralPercentage { get; set; }
        public string CurrentStage { get; set; }
        public string YoutubeUrl { get; set; }
        public List<T1> ReferralDetails { get; set; }
        public List<T2> TopTwoEarners { get; set; }
    }


    public class ResponseBaseModelClientReferral<T>
    {
        public double TotalReferralAmtEarned { get; set; }
        public long ReferralPercentage { get; set; }
        public long TotalReferredClient { get; set; }
        public string CurrentStage { get; set; }
        public string YoutubeUrl { get; set; }
        public bool IsDirect { get; set; } = false;
        public List<T> TopTwoEarners { get; set; }
    }

    public class ResponseBaseModelLeadReferral<T>
    {
        public List<T> ReferralDetails { get; set; }
        public long TotalReferredClient { get; set; }
        public long TotalRows { get; set; }
    }


    public class ResponseBaseModelList<T>
    {
        public long ResponseId { get; set; }
        public string ResponseMessage { get; set; }
        public List<T> Datas { get; set; }
        public long? TotalRows { get; set; }
        public string DownloadFilePath { get; set; }
    }
    // Pledge and  unpledge request and response
    public class ResponseBaseLWMModel<T>
    {
        public long ResponseId { get; set; }
        public string ResponseMessage { get; set; }
        public List<T> Data { get; set; }
    }
    public class ResponseBaseLtrLWMModel<T>
    {
        public int StatusCode { get; set; }
        public bool IsError { get; set; }
        public long ResponseId { get; set; }
        public string ResponseMessage { get; set; }
        public List<T> Data { get; set; }
    }
    public class ResponseBaseItrPLModel<T>
    {
        public int StatusCode { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
        public long TotalRows { get; set; }
        public List<T> Datas { get; set; }
    }
    public class ResponseBaseltrSWMModel<T>
    {
        public int StatusCode { get; set; }
        public bool IsError { get; set; }
        public long ResponseId { get; set; }
        public string ResponseMessage { get; set; }
        public T Data { get; set; }
    }
    public class EkycTokeExpiryResponse
    {
        public string code { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }

}
