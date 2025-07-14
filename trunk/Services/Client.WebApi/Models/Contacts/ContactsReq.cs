using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Client.WebApi.Models
{
    public class InsertContactReq
    {
        [Required]
        public List<Contacts> ContactJSON { get; set; }
    }
    public class Contacts
    {
        public string ClientID { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
    }
}
