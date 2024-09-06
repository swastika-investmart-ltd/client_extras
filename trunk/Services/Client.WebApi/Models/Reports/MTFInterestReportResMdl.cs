using System;

namespace Client.WebApi
{
    public class MTFInterestReportResMdl
    {
		public string Uid { get; set; } //ACCOUNTCODE
		public Decimal Amount { get; set; } //CR_AMT
		public string VoucherDate { get; set; } //VOUCHERDATE
		public string VoucherNo { get; set; } //VOUCHERNO	
	}
}
