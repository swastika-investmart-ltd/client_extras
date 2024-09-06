using System.ComponentModel.DataAnnotations;
using static Entities.CustomValidators;

namespace Client.WebApi
{
    public class MTFInterestReportReqMdl
    {
        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        [ValueInList("CFY", "PFY", ErrorMessage = "Invalid Type.")]
        public string Type { get; set; }

        //[Required(ErrorMessage = "SecurityKey is required.")]
        public string SecurityKey { get; set; }
    }
}
