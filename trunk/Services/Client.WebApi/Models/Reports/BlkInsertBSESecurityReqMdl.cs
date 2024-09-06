using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static Client.WebApi.CustomValidators;

namespace Client.WebApi
{
    public class BlkInsertBSESecurityReqMdl
    {

        [Required(ErrorMessage = "SecurityKey is required.")]
        public string SecurityKey { get; set; }

        [EnsureMinElementsAttribute(1, ErrorMessage = "At least one item required in bse security")]
        public List<BSESecurityReqMdl> BSESecurityList { get; set; }        
    }
    
    public class BSESecurityReqMdl
    {
         public int Token { get; set; }
        public string ScripID { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
