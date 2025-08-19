using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static Entities.CustomValidators;

namespace Client.WebApi
{
    public class WebRecommendation
    {
        public int OrderId { get; set; }
        public string ScripSymbol { get; set; }
        public string ScripToken { get; set; }
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
        public string TradeStatus { get; set; }
        public decimal ProfitLossPercent { get; set; }
        public decimal ProfitLossRs { get; set; }
    }

    public class DailyWebRecommendation
    {
        public DateTime OrderDate { get; set; }
        public decimal NetDayGainPercent { get; set; }
    }

    public class MobRecommendation
    {
        public int OrderId { get; set; }
        public string ScripSymbol { get; set; }
        public string ScripToken { get; set; }
        public string SegmentName { get; set; }
        public string ExchangeName { get; set; }
        public string IntradaybtstDelivery { get; set; }
        public string KBContract { get; set; }
        public decimal PriceRangeFrom { get; set; }
        public decimal PriceRangeTo { get; set; }
        public int Duration { get; set; }
        public string DurationType { get; set; }
        public string MessageType { get; set; }
        public string ScripOption { get; set; }
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
        public string TradeStatus { get; set; }
        public decimal ProfitLossPercent { get; set; }
        public decimal ProfitLossRs { get; set; }
    }


    public class WebCallRecommendation
    {
        public IEnumerable<WebRecommendation> WebCallRecommendations { get; set; }
        public IEnumerable<decimal> DailyGraphRecommendation { get; set; }
        public object GraphCallSummary { get; set; }
    }

    public class MobCallRecommendation
    {
        public IEnumerable<MobRecommendation> MobCallRecommendations { get; set; }
        public IEnumerable<decimal> DailyGraphRecommendation { get; set; }
        public object GraphCallSummary { get; set; }
        public List<ExitDateGroup> GroupedByExitDate { get; set; }

        // Paging info
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class ExitDateGroup
    {
        public string ExitDate { get; set; }
        public decimal? NetDayGainPercent { get; set; }
        public List<MobRecommendation> MobClosedCall { get; set; }
    }

    public class OrderbySegmentsReq
    {
        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }

        [Required(ErrorMessage = "Segment is required.")]
        [ValueInList("All", "EQUITY", "FNO_STOCK", "FNO_INDEX", "COMMODITY", ErrorMessage = "Invalid Segment")]
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
