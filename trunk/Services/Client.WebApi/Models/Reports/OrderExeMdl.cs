using System.ComponentModel.DataAnnotations;

namespace ResearchPanel
{
    public class OrderExeMdl
    {
        [Required(ErrorMessage = "Company Id is required.")]
        public long CompanyId { get; set; }

        [Required(ErrorMessage = "Created By is required.")]
        public long CreatedBy { get; set; }

        [Required(ErrorMessage = "Page No. is required.")]
        public int PageNo { get; set; }

        [Required(ErrorMessage = "Page Size is required.")]
        public int PageSize { get; set; }
    }
}
