using static Entities.CustomValidators;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Client.WebApi
{
    public class CalBrokerageRequest
    {
        [Required(ErrorMessage = "UserId is required.")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Scrip Symbol is required.")]
        public string ScripSymbol { get; set; }

        [Required(ErrorMessage = "Exch is required.")]
        [ValueInList("NSE", "BSE", "NFO", "BFO", "CDS", "BCD", "MCX", "NCX", "NCOM", ErrorMessage = "Invalid exchange.")]
        public string Exch { get; set; } //NSE,BSE,NFO,BFO,CDS,BCD,MCX,NCX

        [Required(ErrorMessage = "OptType is required.")]
        [ValueInList("CASH", "FUT", "OPT", ErrorMessage = "Invalid option type.")]
        public string OptType { get; set; }

        [Required(ErrorMessage = "Prd is required.")]
        [ValueInList("C", "M", "I", "B", "H", "F", ErrorMessage = "Invalid prd type.")]
        public string Prd { get; set; } // C.M - Del / I,B,H - Intra

        [Required(ErrorMessage = "Price is required.")]//Decimal.MaxValue => 79,228,162,514,264,337,593,543,950,335
        [Range(0.0001, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        //[Range(typeof(decimal), "0.0001", "79228162514264337593543950335", ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; } //Price

        [Required(ErrorMessage = "Qty is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Qty must be greater than zero")]
        public int Qty { get; set; } //QTY

        [RequiredIf("OptType", "OPT", ErrorMessage = "OptQty is required.")]
        [RangeIf(1, int.MaxValue, "OptType", "OPT", ErrorMessage = "OptQty must be greater than zero")]
        public int OptQty { get; set; } //Lot QTY

        [Required(ErrorMessage = "TransType is required.")]
        [ValueInList("B", "S", ErrorMessage = "Invalid trans type.")]
        public string TransType { get; set; }

        [Required(ErrorMessage = "MarginReq is required.")]
        //[RegularExpression(@"^(?!.*\..*\.)[.\d]+$", ErrorMessage = "MarginReq is required.")]
        [Range(0.0, double.MaxValue, ErrorMessage = "MarginReq must be equal or greater than zero.")]
        public decimal MarginReq { get; set; }

        [RequiredIf("IsCommodity", true, ErrorMessage = "PrcFactor is required.")]
        [DoubleRangeIf(0.0001, double.MaxValue, "IsCommodity", true, ErrorMessage = "PrcFactor must be greater than zero.")]
        public decimal PrcFactor { get; set; }

        [RequiredIf("IsCommodity", true, ErrorMessage = "Multiplier is required.")]
        [DoubleRangeIf(0.0001, double.MaxValue, "IsCommodity", true, ErrorMessage = "Multiplier must be greater than zero.")]
        public decimal Multiplier { get; set; }

        [NotMapped]
        public bool IsCommodity
        {
            get
            {
                return (this.Exch == "MCX" || this.Exch == "NCX") ? true : false;
            }
        }
    }
}
