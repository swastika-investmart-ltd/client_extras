using System;
using System.ComponentModel.DataAnnotations;
using static Entities.CustomValidators;

namespace Client.WebApi
{
    public class TradeSummaryReqMdl
    {
        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }

        [Required(ErrorMessage = "Segment is required.")]      
        [ValueInList("All", "Equity", "FNO", "Currency", "Commodity", ErrorMessage = "Invalid segment.")]
        public string Segment { get; set; }

        [Required(ErrorMessage = "FromDate is required.")]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "ToDate is required.")]    
        public DateTime ToDate { get; set; }

        //[Required(ErrorMessage = "SecurityKey is required.")]
        public string SecurityKey { get; set; }
    }
}
