using System;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.WebApi
{
    public class LeadReferralRequest
    {
        [Required(ErrorMessage = "ClientId is required.")]
        [RegularExpression(@"^[a-zA-Z0-9\s,]*$", ErrorMessage = "Please enter valid ClientId")]
        public string ClientId { get; set; }

        [RegularExpression(@"^[a-zA-Z\s,]*$", ErrorMessage = "Please enter Search Value")]
        public string SearchValue { get; set; }

        [RegularExpression(@"^[0-9\s,]*$", ErrorMessage = "Please enter valid PageNo")]
        public int PageNo { get; set; } = 1;

        [RegularExpression(@"^[0-9\s,]*$", ErrorMessage = "Please enter valid PageSize")]
        public int PageSize { get; set; } = 5;

    }

    public class ClientReferral
    {

        [Required(ErrorMessage = "ClientId is required.")]
        [RegularExpression(@"^[a-zA-Z0-9\s,]*$", ErrorMessage = "Please enter valid ClientId")]
        public string ClientId { get; set; }

        [Required(ErrorMessage = "DateFilter is required.")]
        [RegularExpression(@"^[a-zA-Z\s,]*$", ErrorMessage = "Please enter valid Date Filter")]
        public string DateFilter { get; set; }
    }

    public class ClientReferralResponse
    {
        public string FullName { get; set; }
        public string Status { get; set; }
        public double IntialMargin { get; set; }
        public string MobileNo { get; set; }
    }

    public class TopTwoEarners
    {
        public string ClientName { get; set; }
        public double AmountEarnered { get; set; }
    }

    public class BrokerageDetails
    {
        public string ClientName { get; set; }
        public string ClientCode { get; set; }
        public DateTime? TradeDate { get; set; }
        public double brokerage { get; set; }
        public string ReferredBy { get; set; }
        public string RMCode { get; set; }
        public bool IsDirect { get; set; }
    }

    public class DirectCustomers
    {
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public bool IsDirect { get; set; }
    }

}
