using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using static Entities.CustomValidators;

namespace ResearchPanel.Entities
{
    public class ScripGeneralResponse
    {
        public long GeneralId { get; set; }
        public bool Equity { get; set; }
        public bool FutureOptions { get; set; }
        public bool Currencies { get; set; }
        public bool Commodities { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedOn { get; set; }
        public long CompanyId { get; set; }
    }
    public class ScripOffersResponse
    {
        public long OfferId { get; set; }
        public string Heading { get; set; }
        public string OfferDetailes { get; set; }
        public string ButtonText { get; set; }
        public string ButtonHyperlink { get; set; }
        public DateTime DurationFrom { get; set; }
        public DateTime DurationTo { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Status { get; set; }
        public bool IsRead { get; set; }
        public bool ReferStatus { get; set; }
        public string FilePath { get; set; }
        public long CompanyId { get; set; }
        //ReferStatus= > IsReferFriend
    }

    public class ScripOrderResponse
    {
        public long OrderId { get; set; }
        public string ScripSymbol { get; set; }
        public string ScripToken { get; set; }
        public string IntradaybtstDelivery { get; set; }
        public string BuySell { get; set; }
        public Decimal PriceRangeFrom { get; set; }
        public Decimal PriceRangeTo { get; set; }
        public Decimal Target { get; set; }
        public Decimal StopLoss { get; set; }
        public long Duration { get; set; }
        public string DurationType { get; set; }
        public DateTime CreatedOn { get; set; }
        public string MessageType { get; set; }
        public string Message { get; set; }
        public string ScripOption { get; set; }
        public string CashToken { get; set; }
        public string StrikePrice { get; set; }
        public string CallOrPut { get; set; }
        public string ExpiryDate { get; set; }
        public string CompanyName { get; set; }
        public string InstrumentName { get; set; }
        public string SegmentName { get; set; }
        public string ExchangeName { get; set; }
        public string IndustryType { get; set; }
        public string Status { get; set; }
        public bool IsRead { get; set; }
        public string FilePath { get; set; }
        public long CompanyId { get; set; }
        public string KBContract { get; set; }
    }

    public class ScripOrderFollowUpResponse
    {
        public long FollowupId { get; set; }
        public string Message { get; set; }
        public long OrderId { get; set; }
        public bool IsButtonDisplayed { get; set; }
        public string ButtonBuySell { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsRead { get; set; }
    }

    public class ScripWithOrderFollowUpResponse
    {
        public long OrderId { get; set; }
        public string ScripSymbol { get; set; }
        public string ScripToken { get; set; }
        public string IntradaybtstDelivery { get; set; }
        public string BuySell { get; set; }
        public Decimal PriceRangeFrom { get; set; }
        public Decimal PriceRangeTo { get; set; }
        public Decimal Target { get; set; }
        public Decimal StopLoss { get; set; }
        public long Duration { get; set; }
        public string DurationType { get; set; }
        public DateTime CreatedOn { get; set; }
        public string MessageType { get; set; }
        public string Message { get; set; }
        public string ScripOption { get; set; }
        public string CashToken { get; set; }
        public string StrikePrice { get; set; }
        public string CallOrPut { get; set; }
        public string ExpiryDate { get; set; }
        public string CompanyName { get; set; }
        public string InstrumentName { get; set; }
        public string SegmentName { get; set; }
        public string ExchangeName { get; set; }
        public string IndustryType { get; set; }
        public string Status { get; set; }
        public string FilePath { get; set; }
        public long CompanyId { get; set; }
        public string KBContract { get; set; }

        //Followup
        public long FollowupId { get; set; }
        public string FollowupMessage { get; set; }
        public DateTime FollowupCreatedOn { get; set; }
        public bool IsButtonDisplayed { get; set; }
        public string ButtonBuySell { get; set; }
        public bool IsRead { get; set; }
    }
    public class AllScripInfoResponse
    {
        //Order[Research]
        public long OrderId { get; set; }
        public string ScripSymbol { get; set; }
        public string ScripToken { get; set; }
        public string IntradaybtstDelivery { get; set; }
        public string BuySell { get; set; }
        public Decimal PriceRangeFrom { get; set; }
        public Decimal PriceRangeTo { get; set; }
        public Decimal Target { get; set; }
        public Decimal StopLoss { get; set; }
        public long Duration { get; set; }
        public string DurationType { get; set; }
        public string MessageType { get; set; }
        public string Message { get; set; }
        public string ScripOption { get; set; }
        public string CashToken { get; set; }
        public string StrikePrice { get; set; }
        public string CallOrPut { get; set; }
        public string ExpiryDate { get; set; }
        public string CompanyName { get; set; }
        public string InstrumentName { get; set; }
        public string SegmentName { get; set; }
        public string ExchangeName { get; set; }
        public string IndustryType { get; set; }
        public string FilePath { get; set; }
        public long CompanyId { get; set; }
        public string KBContract { get; set; }

        //Followup
        public long FollowupId { get; set; }
        public string FollowupMessage { get; set; }
        public DateTime FollowupCreatedOn { get; set; }
        public bool IsButtonDisplayed { get; set; }
        public string ButtonBuySell { get; set; }

        //For Offer
        public long OfferId { get; set; }
        public string Heading { get; set; }
        public string OfferDetailes { get; set; }
        public string ButtonText { get; set; }
        public string ButtonHyperlink { get; set; }
        public DateTime DurationFrom { get; set; }
        public DateTime DurationTo { get; set; }
        public bool ReferStatus { get; set; }

        //For Ipo
        public long IpoId { get; set; }
        public string Name { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        //For General
        public long GeneralId { get; set; }
        public bool Equity { get; set; }
        public bool FutureOptions { get; set; }
        public bool Currencies { get; set; }
        public bool Commodities { get; set; }
        public string Subject { get; set; }
        //Change Message to GeneralMessage
        public string GeneralMessage { get; set; }

        //Common In ALL
        public DateTime CreatedOn { get; set; }
        public string Status { get; set; }
        public string ResearchType { get; set; }
        public bool IsRead { get; set; }

        //For Order By
        public DateTime ShortDate { get; set; }
    }

    //public class LogInOutInfoPost
    //{
    //    [Required(ErrorMessage = "UserId is required.")]
    //    public string UserId { get; set; } //User Id

    //    [Required(ErrorMessage = "SourceId is required.")]
    //    public string SourceId { get; set; } //DeviceId Or WebUniqueId

    //    [Required(ErrorMessage = "SourceType is required.")]
    //    public string SourceType { get; set; } //web Or mobile    

    //    [Required(ErrorMessage = "ReqType is required.")]
    //    public string ReqType { get; set; } // login Or logout

    //    [Required(ErrorMessage = "SecurityKey is required.")]
    //    public string SecurityKey { get; set; } //Security Key

    //    [Required(ErrorMessage = "CompanyId is required.")]
    //    public string CompanyId { get; set; }
    //}

    public class ScripOrderbySegmentsRes
    {
        public long OrderId { get; set; }
        public string ScripSymbol { get; set; }
        public string ScripToken { get; set; }
        public string IntradaybtstDelivery { get; set; }
        public string BuySell { get; set; }
        public decimal PriceRangeFrom { get; set; }
        public decimal PriceRangeTo { get; set; }
        public decimal Target { get; set; }
        public decimal StopLoss { get; set; }
        public long Duration { get; set; }
        public string DurationType { get; set; }
        public DateTime CreatedOn { get; set; }
        public string MessageType { get; set; }
        public string Message { get; set; }
        public string ScripOption { get; set; }
        public string CashToken { get; set; }
        public string StrikePrice { get; set; }
        public string CallOrPut { get; set; }
        public string ExpiryDate { get; set; }
        public string CompanyName { get; set; }
        public string InstrumentName { get; set; }
        public string SegmentName { get; set; }
        public string ExchangeName { get; set; }
        public string IndustryType { get; set; }
        public string Status { get; set; }
        public bool IsRead { get; set; }
        public string FilePath { get; set; }
        public long CompanyId { get; set; }
        public string KBContract { get; set; }
        public decimal TargetPer { get; set; }
        public decimal SLPer { get; set; }
        public string OrderMargin { get; set; }
    }

    public class ViewRecPercentageInfo
    {
        public long RecPerId { get; set; }
        public decimal StStocksPercent { get; set; }
        public int StStocksCalls { get; set; }
        public decimal StFnoPercent { get; set; }
        public int StFnoCalls { get; set; }
        public decimal StCurrencyPercent { get; set; }
        public int StCurrencyCalls { get; set; }
        public decimal StCommodityPercent { get; set; }
        public int StCommodityCalls { get; set; }
        public decimal LtSwastikaReturns { get; set; }
        public decimal LtNiftyReturns { get; set; }
        public int LtActiveCalls { get; set; }
    }
    public class ScripOrderbySegmentsReq
    {
        //[Required(ErrorMessage = "SecurityKey is required.")]
        public string SecurityKey { get; set; }

        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }

        [Required(ErrorMessage = "Segment is required.")]

        //var segments = new[] { "All", "Equity", "FNO", "Currency", "Commodity", "PreLogin" };
        [ValueInList("All", "Equity", "FNO", "Currency", "Commodity", "PreLogin", ErrorMessage = "Invalid Segment")]
        public string Segment { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        //var Type = new[] { "All", "Intraday", "Delivery" };
        [ValueInList("All", "Intraday", "Delivery", ErrorMessage = "Invalid Type")]
        public string Type { get; set; }

        [Required(ErrorMessage = "PageNo is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "PageNo Id is required.")]
        public int PageNo { get; set; }

        [Required(ErrorMessage = "PageSize is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "PageSize Id is required.")]
        public int PageSize { get; set; }

        public long CompanyId { get; set; }
    }
    public class GSGeneralReq
    {
        [Range(0, int.MaxValue, ErrorMessage = "Invalid CompanyId")]
        public long CompanyId { get; set; }
    }

    public class GSGeneralInfoReq
    {
        [Range(0, int.MaxValue, ErrorMessage = "Invalid CompanyId")]
        public long CompanyId { get; set; }

        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }
    }
    public class GSOffersReq
    {
        [Range(0, int.MaxValue, ErrorMessage = "Invalid CompanyId")]
        public long CompanyId { get; set; }
    }

    public class GSOffersInfoReq
    {
        [Range(0, int.MaxValue, ErrorMessage = "Invalid CompanyId")]
        public long CompanyId { get; set; }

        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }
    }
    public class GSOrderFollowupReq
    {
        [Required(ErrorMessage = "OrderId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid OrderId")]
        public long OrderId { get; set; }
    }
    public class GOrderFollowupReq
    {
        [Required(ErrorMessage = "OrderId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid OrderId")]
        public long OrderId { get; set; }

        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }
    }

    public class GAllScripInfoReq
    {
        [Range(0, int.MaxValue, ErrorMessage = "Invalid CompanyId")]
        public long CompanyId { get; set; }

        [Required(ErrorMessage = "UId is required.")]
        public string UId { get; set; }
    }
    public class GAllScripInfoPaginationReq
    {
        [Range(0, int.MaxValue, ErrorMessage = "Invalid CompanyId")]
        public long CompanyId { get; set; }

        [Required(ErrorMessage = "PageNo is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid PageNo")]
        public long PageNo { get; set; }

        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }
    }
    public class TopRecommLstReq
    {
        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }
    }

    //public class TopRecommLstIntrReq
    //{
    //    [Required(ErrorMessage = "Uid is required.")]
    //    public string Uid { get; set; }

    //    [Required(ErrorMessage = "SecurityKey is required.")]
    //    public string SecurityKey { get; set; }
    //}
    public class ViewRecomReq
    {
        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }
    }
}