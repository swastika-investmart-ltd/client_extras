using System.Collections.Generic;

namespace Client.WebApi
{
    public class HoldingTradeSummaryResMdl
    {
        public string TradeDate { get; set; }
        public int TotalRows { get; set; }
        public List<HoldingTradeSummaryDataResMdl> SummaryList { get; set; }
    }

    public class HoldingTradeSummaryIntrlResMdl
    {
        public string TRADE_DATE { get; set; }
        public string RowText { get; set; }
        public string BuySell { get; set; }
    }

    public class HoldingTradeSummaryDataResMdl
    {
        public string BuySell { get; set; }
        public string RowText { get; set; }
    }
}
