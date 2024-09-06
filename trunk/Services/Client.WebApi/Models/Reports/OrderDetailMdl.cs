
using System;
using System.ComponentModel.DataAnnotations;

namespace ResearchPanel
{ 
    public class OrderDetailMdl
    {
        [Required(ErrorMessage = "Recommendation Id is required.")]
        [Range(1, long.MaxValue, ErrorMessage = "RecommendationId is required.")]
        public long RecommendationId { get; set; }

        [Required(ErrorMessage = "Uid is required.")]
        public string Uid { get; set; }  //User id of the logged in user.

        [Required(ErrorMessage = "SecurityKey is required.")]
        public string SecurityKey { get; set; }        
    }
}
