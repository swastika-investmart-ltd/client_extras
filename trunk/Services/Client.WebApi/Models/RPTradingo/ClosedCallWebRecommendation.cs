using Client.WebApi.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static Entities.CustomValidators;

namespace Client.WebApi
{
    public class WebCallRecommendation
    {
        public int OrderId { get; set; }
        public string ScripToken { get; set; }
        public string ScripSymbol { get; set; }       
        public string SegmentName { get; set; }
        public string ExchangeName { get; set; }
        public string IntradaybtstDelivery { get; set; }
        public string KBContract { get; set; }
        public decimal PriceRangeFrom { get; set; }
        public decimal PriceRangeTo { get; set; }
        public string CompanyName { get; set; }
        public string TradeType { get; set; }
        public string EntryPrice { get; set; }
        public decimal ClosingPrice { get; set; }
        public decimal Target { get; set; }
        public decimal StopLoss { get; set; }
        public string BuySell { get; set; }
        public string Status { get; set; }
        public string OrderMargin { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime? ExitDate { get; set; }
        public string DaysDifference { get; set; }   
        public decimal ProfitLossPercent { get; set; }
        public decimal ProfitLossRs { get; set; }       
    }

    public class WebGraphData
    {
        public List<DailyWebRecommendation> GraphPerformance {  get; set; }
    }

    public class GraphCallStatics
    {
        public Decimal PositiveCalls { get; set; }
        public Decimal TotalCalls { get; set; }
        public Decimal PositiveCallPercent { get; set; }
    }

    public class DailyWebIntrlRecommendation
    {
        public DateTime OrderClosedDate { get; set; }
        public decimal NetDayGainPercent { get; set; }
    }
    public class DailyWebRecommendation
    {
        public string OrderClosedDate { get; set; }
        public decimal NetDayGainPercent { get; set; }
    }

    public class MobCallRecommendation
    {
        public int OrderId { get; set; }
        public string ScripSymbol { get; set; }
        public string ScripToken { get; set; }
        public string ScripOption { get; set; }
        public string SegmentName { get; set; }
        public string ExchangeName { get; set; }
        public string IntradaybtstDelivery { get; set; }
        public string KBContract { get; set; }
        public decimal PriceRangeFrom { get; set; }
        public decimal PriceRangeTo { get; set; }
        public string CompanyName { get; set; }
        public string TradeType { get; set; }
        public string EntryPrice { get; set; }
        public decimal ClosingPrice { get; set; }
        public decimal Target { get; set; }
        public decimal StopLoss { get; set; }
        public string BuySell { get; set; }
        public string Status { get; set; }
        public int Duration { get; set; }
        public string DurationType { get; set; }
        public string MessageType { get; set; }
        public string OrderMargin { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime? ExitDate { get; set; }
        public string DaysDifference { get; set; }
        public decimal ProfitLossPercent { get; set; }
        public decimal ProfitLossRs { get; set; }
        public DateTime CreatedOn { get; set; }
        public string StrikePrice { get; set; }
        public string ExpiryDate { get; set; }
        public string InstrumentName { get; set; }
        public string IndustryType { get; set; }
    }

    public class MobGraphData
    {
        public string MinDate { get; set; }
        public string MaxDate { get; set; }        
        public List<decimal> GraphPerformance { get; set; }
    }

    public class DailyMobRecommendation
    {
        public DateTime OrderClosedDate { get; set; }
        public decimal NetDayGainPercent { get; set; }
    }

    public class ClosedData
    {
        public string ExitDate { get; set; }
        public decimal? NetDayGainPercent { get; set; }
        public List<MobCallRecommendation> ClosedList { get; set; }
    }

    public class OrderbySegmentsReq
    {
        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }

        [Required(ErrorMessage = "Segment is required.")]
        [SegmentListValidation(ErrorMessage = "Invalid Segment or combination.")]
        public string Segment { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        [ValueInList("All", "Intraday", "Delivery", ErrorMessage = "Invalid Type")]
        public string Type { get; set; }

        [ValueInList("Live", "Closed", ErrorMessage = "Invalid Call Status")]
        public string CallStatus { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
