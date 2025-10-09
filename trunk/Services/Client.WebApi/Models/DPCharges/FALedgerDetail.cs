using System;

namespace Client.WebApi
{
    public class LedgerAPIResponse
    {
        public string COCD { get; set; }
        public string CONAME { get; set; }
        public string KINDOFACCOUNT { get; set; }
        public string ACCOUNTCODE { get; set; }
        public string ACCOUNTNAME { get; set; }
        public string TELNO { get; set; }
        public string FAX { get; set; }
        public string ADDR { get; set; }
        public string OPENINGBALANCE { get; set; }
        public string DR_AMT { get; set; }
        public string CR_AMT { get; set; }
        public string VOUCHERDATE { get; set; }
        public string SETTLEMENT_NO { get; set; }
        public string CTRCODE { get; set; }
        public string CTRNAME { get; set; }
        public string TRANS_TYPE { get; set; }
        public string VOUCHERNO { get; set; }
        public string NARRATION { get; set; }
        public string BILLNO { get; set; }
        public string CHQNO { get; set; }
        public string EXPECTED_DATE { get; set; }
        public string TRADING_COCD { get; set; }
        public string PANNO { get; set; }
        public string EMAIL { get; set; }
        public string MANUALVNO { get; set; }
        public string BOOKTYPECODE { get; set; }
        public string BILL_DATE { get; set; }
        public string MKT_TYPE { get; set; }
        public string GROUPCODE { get; set; }
        public string BRSFLAG { get; set; }
        public string SETL_PAYINDATE { get; set; }
        public string LAST2SETL { get; set; }
        public string ACCOUNTCODE1 { get; set; }
        public string GATEWAYID { get; set; }
        public string PUNCH_TIME { get; set; }
        public string voctype { get; set; }
        public string CHQIMAGEPATH { get; set; }
        public string TRANS_TYPE1 { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string STARTYEAR { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
