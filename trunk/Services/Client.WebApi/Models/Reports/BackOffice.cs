using System.ComponentModel.DataAnnotations;

namespace Client.WebApi
{ 
    public class ClientInfoPost
    {
        [Required(ErrorMessage = "SecurityKey is required.")]
        public string SecurityKey { get; set; }

        [Required(ErrorMessage = "UserId is required.")]
        public string UserId { get; set; } //UserId  
    }
}
