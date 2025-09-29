namespace Client.WebApi
{
    public class CalBrokerageResponse
    {
        public string Brokerage { get; set; }
        //public string MarginReq { get; set; }
        public string Total { get; set; }
        public string STT { get; set; }
        public string StampDuty { get; set; }
        public string TurnoverChgs { get; set; }
        public string SEBIFees { get; set; }
        public string CMCharges { get; set; }
        public string CTT { get; set; }
        public string RMFFees { get; set; }
        public string GST { get; set; }
    }
}
