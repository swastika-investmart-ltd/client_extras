using System.Collections.Generic;

namespace Client.WebApi
{
 //   public class AnnualPnlSummaryResMdl
 //   {
	//	public string CLIENT_ID { get; set; } //UCC Code or Account Code or Client ID
	//	public string PL_AMT { get; set; } //Net Amount or Profit/Loss Amount
	//	public string BUY_QTY { get; set; } //Buy Qty
	//	public string BUY_RATE { get; set; } //Buy Rate
	//	public string BUY_AMT { get; set; } //Buy Amount
	//	public string SALE_QTY { get; set; } //Sell Qty
	//	public string SALE_AMT { get; set; } //Sell Amount
	//	public string SALE_RATE { get; set; } //Sell Rate
	//	public string LONG_TERM { get; set; } //Long Term Stock
	//	public string SHORT_TERM { get; set; } //Short Term Stock
	//	public string SPECULATION { get; set; } //Investment Stock
	//	public string TR_TYPE { get; set; } //Transaction type
	//	public string CURR_AMOUNT { get; set; } //With Current Closing Price Stock Amount
	//	public string CLIENT_NAME { get; set; } //Client Name
	//	public string SCRIP_SYMBOL1 { get; set; } //Scrip symbol
	//	public string SCRIP_NAME { get; set; } //Scrip Name
	//	public string NET_QTY { get; set; }  //Net Quantity                               
	//	public string NET_RATE { get; set; } //Net rate
	//	public string NET_AMOUNT { get; set; } //Net Amount
	//	public string CLOSING_PRICE { get; set; } //Closing Price
	//}

	public class AnnualPnlSummaryResponseMdl
	{
		//public string Uid { get; set; } //CLIENT_ID - UCC Code or Account Code or Client ID
		public decimal PlAmt { get; set; } //Net Amount or Profit/Loss Amount
		public int BuyQty { get; set; } //Buy Qty
		public decimal BuyRate { get; set; } //Buy Rate
		public decimal BuyAmt { get; set; } //Buy Amount
		public int SellQty { get; set; } //Sell Qty
		public decimal SellAmt { get; set; } //Sell Amount
		public decimal SellRate { get; set; } //Sell Rate
		//public string LongTerm { get; set; } //Long Term Stock
		//public string ShortTerm { get; set; } //Short Term Stock
		//public string Speculation { get; set; } //Investment Stock
		public string TrType { get; set; } //Transaction type
		//public string CurrAmount { get; set; } //With Current Closing Price Stock Amount
		//public string ClientName { get; set; } //Client Name
		public string ScripSymbol { get; set; } //Scrip symbol
		public string ScripName { get; set; } //Scrip Name
		public int NetQty { get; set; }  //Net Quantity                               
		//public string NetRate { get; set; } //Net rate
		//public string NetAmount { get; set; } //Net Amount
		public string ClosingPrice { get; set; } //Closing Price
	}

	public class AnnualPnlSummaryResMdl
	{		
		public string ScripName { get; set; }
		public string ScripSymbol { get; set; }
		public int TotalRows { get; set; }
		public List<SummaryList> SummaryList { get; set; }
	}

	public class AnnualPnlSummaryExpResMdl
	{
		public string ScripName { get; set; }
		public string ScripSymbol { get; set; }
		public int TotalRows { get; set; }
		public List<SummaryExpensesList> SummaryList { get; set; }
	}


	public class SummaryList
	{
		public decimal PlAmt { get; set; } //Net Amount or Profit/Loss Amount
		public int BuyQty { get; set; } //Buy Qty
		public decimal BuyRate { get; set; } //Buy Rate
		public decimal BuyAmt { get; set; } //Buy Amount
		public int SellQty { get; set; } //Sell Qty
		public decimal SellAmt { get; set; } //Sell Amount
		public decimal SellRate { get; set; } //Sell Rate										
		public string TrType { get; set; } //Transaction type
		public int NetQty { get; set; }  //Net Quantity  
		public string ClosingPrice { get; set; } //Closing Price
	}

	public class SummaryExpensesList
	{
		public decimal PlAmt { get; set; } //Net Amount or Profit/Loss Amount
		public int BuyQty { get; set; } //Buy Qty
		public decimal BuyRate { get; set; } //Buy Rate
		public decimal BuyAmt { get; set; } //Buy Amount
		public int SellQty { get; set; } //Sell Qty
		public decimal SellAmt { get; set; } //Sell Amount
		public decimal SellRate { get; set; } //Sell Rate										
		public string ScripName { get; set; } //Scrip Name
		public int NetQty { get; set; }  //Net Quantity  
		public string ClosingPrice { get; set; } //Closing Price
	}


}
