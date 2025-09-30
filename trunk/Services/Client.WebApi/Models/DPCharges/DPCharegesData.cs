using System;

namespace Client.WebApi
{
    public class LedgerResponse
    {
        public long ID { get; set; }                    // Id
        public string BO_ID { get; set; }              // BO_ID (1204370000000571)
        public string BO_NAME { get; set; }            // BO_NAME (Govind Chawla)
        public DateTime VOUCHERDATE { get; set; }       // VOUCHERDATE (2025-09-19)
        public DateTime TransTime { get; set; }       // TRANSTIME (2025-09-19 00:00:00.000)
        public DateTime IMPORTTIME { get; set; }      // IMPORTTIME (2025-09-20 08:26:53.827)
        public string SCRIP_NAME { get; set; }         // SCRIP_NAME (ADANI TOTAL GAS LIMITED)
        public decimal DEBIT_QTY { get; set; }         // DEBIT_QTY (3.0000)
        public decimal CREDIT_QTY { get; set; }        // CREDIT_QTY (0.0000)
        public decimal QTY { get; set; }              // QTY (3.0000)
        public decimal DP_CHARGE { get; set; }         // DP_CHARGE (0.0000)
        public string DP_COMMENT { get; set; }         // DP_COMMENT (EARLY PAY IN CHARGE)
        public string NEW_NARRATION { get; set; }      // NEW_NARRATION (EARLY PAY IN CHARGE)
        public string CATEGORY { get; set; }  // DPCHARGE_CATEGORY (STOCK SELLING CHARGES)
        public string SUB_CATEGORY { get; set; }  // DPCHARGE_CATEGORY (STOCK SELLING CHARGES)
        public string NARRATION { get; set; }         // NARRATION (IREM Txn:63908473 ...)

        public override string ToString()
        {
            return $"ID:{ID}, VOUCHERDATE:{VOUCHERDATE}, DP_CHARGE:{DP_CHARGE}, QTY:{QTY} CATEGORY:{CATEGORY}, SUB_CATEGORY:{SUB_CATEGORY}, NEW_NARRATION:{NEW_NARRATION}, SCRIP_NAME: {SCRIP_NAME}";
        }
    }

    public class LedgerFADataResponse
    {
        public string ACCOUNTCODE { get; set; }
        public string ACCOUNTNAME { get; set; }
        public string DR_AMT { get; set; }
        public string CR_AMT { get; set; }
        public string VOUCHERNO { get; set; }
        public string VOUCHERDATE { get; set; }
        public string BILL_DATE { get; set; }
        public string PUNCH_TIME { get; set; }
        public string TRANS_TYPE { get; set; }
        public string NARRATION { get; set; }

        public override string ToString()
        {
            return $"ACCOUNTCODE:{ACCOUNTCODE}, VOUCHERDATE:{VOUCHERDATE}";
        }
    }
}
