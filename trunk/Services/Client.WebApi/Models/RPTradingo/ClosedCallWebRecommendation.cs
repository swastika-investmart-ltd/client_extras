using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static Entities.CustomValidators;

namespace Client.WebApi
{
    public class WebRecommendation
    {
        public string CompanyName { get; set; }
        public string TradeType { get; set; }
        public string EntryPrice { get; set; }
        public decimal ClosingPrice { get; set; }
        public decimal Target { get; set; }
        public decimal StopLoss { get; set; }
        public string BuySell { get; set; }
        public string Status { get; set; }
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

    public class WebCallRecommendation
    {
        public IEnumerable<WebRecommendation> WebCallRecommendations { get; set; }
        public List<DailyWebRecommendation> DailyGraphRecommendation { get; set; }
        public object GraphCallSummary { get; set; }
    }

    public class OrderbySegmentsReq
    {
        [Required(ErrorMessage = "Segment is required.")]
        [ValueInList("All", "EQUITY", "FNO_STOCK", "FNO_INDEX", "COMMODITY", ErrorMessage = "Invalid Segment")]
        public string Segment { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        [ValueInList("All", "Intraday", "Delivery", ErrorMessage = "Invalid Type")]
        public string Type { get; set; }

        [ValueInList("All", "Live", "Closed", ErrorMessage = "Invalid Call Status")]
        public string CallStatus { get; set; }
    }
}
