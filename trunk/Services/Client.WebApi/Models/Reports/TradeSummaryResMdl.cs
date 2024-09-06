using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Client.WebApi
{
    public class TradeSummaryResMdl
    {
        public string TradeDate { get; set; }
        public int TotalRows { get; set; }
        public List<TradeSummaryDataResMdl> SummaryList { get; set; }
    }

    public class TradeSummaryDataResMdl
    {
        public string CompanyCode { get; set; }
        public string ScripSymbol { get; set; }
        public string ScripName { get; set; }
        public string BuySale { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string TradeDate { get; set; }
        public string TradeTime { get; set; }   
        public DateTime TradeDateTime { get; set; }
    }

    //public class TradeSummaryIntrlResMdl
    //{
    //    public string COMPANY_CODE { get; set; }
    //    public string SCRIP_SYMBOL { get; set; }
    //    public string SCRIP_NAME { get; set; }
    //    public string BUY_SALE { get; set; }
    //    public int QUANTITY { get; set; }
    //    public decimal PRICE_PREMIUM { get; set; }
    //    public string TRADE_DATE { get; set; }
    //    public string TRADE_TIME { get; set; }
    //    public DateTime TRADE_DATETIME { get; set; }       
    //}
}
