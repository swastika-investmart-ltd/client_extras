using System.ComponentModel.DataAnnotations;

namespace Client.WebApi
{
    public class LoadBSESecurityReqMdl
    {
        [Required(ErrorMessage = "SecurityKey is required.")]
        public string SecurityKey { get; set; }
    }
}
