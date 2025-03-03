using System;
using System.ComponentModel.DataAnnotations;

namespace Client.WebApi
{
   public class DownLoadReportReqMdl
    {
        //[Required(ErrorMessage = "SecurityKey is required.")]
        public string SecurityKey { get; set; }

        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }

        [Required(ErrorMessage = "Client Name is required.")]
        public string CName { get; set; }

        [Required(ErrorMessage = "Client Email is required.")]
        public string CEmail { get; set; }

        [Required(ErrorMessage = "PanNumber is required.")]
        public string Pan { get; set; }

        [Required(ErrorMessage = "FinYear is required.")]

        [Range(1900, 9999, ErrorMessage = "Please enter a valid year")]
        public long FinYear { get; set; }

        public bool IsEmail { get; set; } = true;


    }

    public class MCXUnderlyingInfoResponse
    {
        public string Ex { get; set; } //Exchange  
        public string Ic { get; set; } //InstCode
        public string Pn { get; set; } //PriceNumerator
        public string Pd { get; set; } //PriceDenominator
        public string Gn { get; set; } //GeneralNumerator
        public string Gd { get; set; } //GeneralDenominator
        public string Ls { get; set; } //GeneralNumerator
        public string Lw { get; set; } //GeneralDenominator
    }
}
