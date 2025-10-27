namespace Client.WebApi
{
    public class CalBrokerageResp
    {
        public decimal Brokerage { get; set; }
        public decimal MarginReq { get; set; } //MarginReq = (MarginReq - Brokerage)
        public decimal Total { get; set; } //SUM(OthCharges) + MarginReq + Brokerage
        public decimal STT { get; set; }
        public decimal StampDuty { get; set; }
        public decimal TurnoverChgs { get; set; }
        public decimal SEBIFees { get; set; }
        public decimal CMCharges { get; set; }
        public decimal CTT { get; set; }
        public decimal RMFFees { get; set; }
        public decimal GST { get; set; }
    }
}
