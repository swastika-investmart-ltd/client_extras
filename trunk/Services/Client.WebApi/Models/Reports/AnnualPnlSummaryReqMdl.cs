using System.ComponentModel.DataAnnotations;

namespace Client.WebApi
{
    public class AnnualPnlSummaryReqMdl
    {
        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }

        [Required(ErrorMessage = "FinYear is required.")]

        [Range(1900, 9999, ErrorMessage = "Please enter a valid year")]
        public long FinYear { get; set; }

        //[Required(ErrorMessage = "SecurityKey is required.")]
        public string SecurityKey { get; set; }
    }
}
