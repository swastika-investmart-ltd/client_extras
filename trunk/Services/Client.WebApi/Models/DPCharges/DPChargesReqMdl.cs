using System.ComponentModel.DataAnnotations;

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
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string StartYear { get; set; }
    }

    public class LedgerRequest
    {
        //[Required(ErrorMessage = "UserId is required.")]
        public string Uid { get; set; }

        //[Required(ErrorMessage = "CategoryId is required.")]
        //[Range(1, 3, ErrorMessage = "CategoryId must be in range 1 to 3.")]
        public long CategoryId { get; set; }

        //[Required(ErrorMessage = "SubCategoryId is required.")]
        public long SubCategoryId { get; set; }
    }
}
