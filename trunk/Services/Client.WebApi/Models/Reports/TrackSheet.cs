using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ResearchPanel
{
    public class TrackSheetReq
    {
        public long CompanyId { get; set; }
        public string DateFlag { get; set; } 
        public string Segment { get; set; }
        public string OrderType { get; set; }
        public string CreatedBy { get; set; }
        public string TeleChannel { get; set; }
        [Required(ErrorMessage = "Page No. is required.")]
        public int PageNo { get; set; }

        [Required(ErrorMessage = "Page Size is required.")]
        public int PageSize { get; set; }
    }

    public class TrackSheetResp
    {
        public string orderId { get; set; }
        public string scripSymbol { get; set; }
        //public string scripToken { get; set; }
        //public string scripOption { get; set; }
        //public string cashToken { get; set; }
        public string strikePrice { get; set; }  /// <summary>
                                                 ///strikePrice  Converted to string bcoz it is giving error due to data
                                                 /// </summary>
        //public string callOrPut { get; set; }
        //public string expiryDate { get; set; }
        // public string instrumentName { get; set; }
        public string segmentName { get; set; }
        public string exchangeName { get; set; }
        public string companyName { get; set; }
        //public string industryType { get; set; }
        public string intradaybtstDelivery { get; set; }
        public string buySell { get; set; }
        public decimal priceRangeFrom { get; set; }
        public decimal priceRangeTo { get; set; }
        public decimal target { get; set; }
        public decimal stopLoss { get; set; }
        //public string duration { get; set; }
        //public string durationType { get; set; }
        //public string messageType { get; set; }
        //public string message { get; set; }
        // public string deliveryContent { get; set; }
        public string status { get; set; }
        public long createdBy { get; set; }
        public DateTime createdOn { get; set; }
        //public bool isRead { get; set; }
        //public string filePath { get; set; }
        public string createdByName { get; set; }
        public long companyId { get; set; }
        // public string companyRName { get; set; }
        //  public string KBContract { get; set; }
        public decimal Target2 { get; set; }
        public string Stage { get; set; }
        public decimal ExitPrice { get; set; }
        public decimal LTP { get; set; }
        public int Quantity { get; set; }
        public string RiskReward { get; set; }
        public string ResearcherName { get; set; } 
        public string ChannelName { get; set; } 
        public string NetPL { get; set; } 
    }
}
