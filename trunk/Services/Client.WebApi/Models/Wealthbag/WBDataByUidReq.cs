using System.ComponentModel.DataAnnotations;

namespace Client.WebApi
{
    public class WBDataByUidReq
    {
        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }
    }
}
