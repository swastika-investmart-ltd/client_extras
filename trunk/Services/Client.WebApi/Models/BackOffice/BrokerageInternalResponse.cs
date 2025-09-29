namespace Client.WebApi
{
    public class BrokerageInternalResponse
    {
        public string CLIENT_ID { get; set; }
        public string EXCHANGE { get; set; }
        public string MODULE_NO { get; set; }
        public string DELIVERYPER { get; set; }
        public string Type { get; set; }
        public string IBT_Module { get; set; }
    }

    public class ScripWiseBrokerageInternalResp
    {
        public string CLIENT_ID { get; set; }
        public string SCRIP_SYMBOL { get; set; }
        public string COMPANY_CODE { get; set; }
        public string SCRIP_TYPE { get; set; }
        public string BuyRate { get; set; }
        public string SellRate { get; set; }
    }
}
