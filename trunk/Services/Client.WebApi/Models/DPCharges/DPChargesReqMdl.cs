using System.ComponentModel.DataAnnotations;
using System.Runtime.Intrinsics.Arm;

namespace Client.WebApi
{
    public class DPChargesReqMdl
    {
        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }
        
        [Required(ErrorMessage = "FinYear is required.")]

        [Range(1900, 9999, ErrorMessage = "Please enter a valid year")]
        public long FinYear { get; set; }
    }

    public class LedgerInternalRequest
    {
        public string ClientCode { get; set; }
        public long CategoryId { get; set; }
        public long SubCategoryId { get; set; }
        public string FundsUtilisedIn { get; set; }
        public string FundsUtilisedFor { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string StartYear { get; set; }
    }

    public class LedgerRequest
    {
        [Required(ErrorMessage = "UserId is required.")]
        public string Uid { get; set; }

        [Required(ErrorMessage = "CategoryId is required.")]
        [Range(1, 3, ErrorMessage = "CategoryId must be in range 1 to 3.")]
        public long CategoryId { get; set; }

        [Required(ErrorMessage = "SubCategoryId is required.")]
        public long SubCategoryId { get; set; }

        //1	All
        //5	DP Charges
        //2	Funds Added
        //3	Funds Withdrawn
        //4	Funds Utilised
    }

    public class FULedgerRequest
    {
        [Required(ErrorMessage = "UserId is required.")]
        public string Uid { get; set; }

        [Required(ErrorMessage = "FundsUtilisedIn is required.")]
        public string FundsUtilisedIn { get; set; }

        [Required(ErrorMessage = "FundsUtilisedFor is required.")]
        public string FundsUtilisedFor { get; set; }

        // 8  Equity	        4 Funds Utilised In
        // 9  Commodity	        4 Funds Utilised In
        // 10 Futures Options	4 Funds Utilised In
        // 11 Currency          4 Funds Utilised In
        // 12 Mutual Funds      4 Funds Utilised In

        // 13 Money Debited  	4 Funds Utilised For
        // 14 Money Credited	4 Funds Utilised For
        // 15 Misc Charges	    4 Funds Utilised For
    }

    public class FULedgerInternalRequest
    {
        public string ClientCode { get; set; }
        public string FundsUtilisedIn { get; set; }
        public string FundsUtilisedFor { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string StartYear { get; set; }
    }
}
