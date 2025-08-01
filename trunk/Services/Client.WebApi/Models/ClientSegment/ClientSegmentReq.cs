using System.ComponentModel.DataAnnotations;

namespace Client.WebApi.Models.ClientSegment
{
    public class SegmentReq
    {
        [Required(ErrorMessage = "Segment is required.")]
        public string Segment { get; set; }
    }
    public class ClientSegment
    {        
        public string ClientId { get; set; }
        public string Segment { get; set; }
    }
}
