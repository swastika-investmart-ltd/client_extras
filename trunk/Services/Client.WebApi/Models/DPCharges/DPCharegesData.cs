using System;
using System.Collections.Generic;

namespace Client.WebApi
{
    public class LedgerResponse
    {
        public long iD { get; set; }                    // Id
        public string BoId { get; set; }              // BO_ID (1204370000000571)
        public string BoName { get; set; }            // BO_NAME (Govind Chawla)
        public DateTime VOUCHERDATE { get; set; }       // VOUCHERDATE (2025-09-19)
        public DateTime TransTime { get; set; }       // TRANSTIME (2025-09-19 00:00:00.000)
        public DateTime ImportTime { get; set; }      // IMPORTTIME (2025-09-20 08:26:53.827)
        public string ScripName { get; set; }         // SCRIP_NAME (ADANI TOTAL GAS LIMITED)
        public decimal DebitQty { get; set; }         // DEBIT_QTY (3.0000)
        public decimal CreditQty { get; set; }        // CREDIT_QTY (0.0000)
        public decimal Qty { get; set; }              // QTY (3.0000)
        public decimal DpCharge { get; set; }         // DP_CHARGE (0.0000)
        public string DpComment { get; set; }         // DP_COMMENT (EARLY PAY IN CHARGE)
        public string NewNarration { get; set; }      // NEW_NARRATION (EARLY PAY IN CHARGE)
        public string Category { get; set; }  // DPCHARGE_CATEGORY (STOCK SELLING CHARGES)
        public string Sub_Category { get; set; }  // DPCHARGE_CATEGORY (STOCK SELLING CHARGES)
        public string Narration { get; set; }         // NARRATION (IREM Txn:63908473 ...)
    }
}
