using System.ComponentModel.DataAnnotations;
using static Entities.CustomValidators;

namespace Client.WebApi
{
    public class HoldingTradeSummaryReqMdl
    {
        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }

        [Required(ErrorMessage = "ScripSymbol is required.")]
        public string ScripSymbol { get; set; }        

        [Required(ErrorMessage = "Type is required.")]
        [ValueInList("All", "Buy", "Sell", ErrorMessage = "Invalid Type.")]
        public string Type { get; set; }

        //[Required(ErrorMessage = "SecurityKey is required.")]
        public string SecurityKey { get; set; }
    }
}
