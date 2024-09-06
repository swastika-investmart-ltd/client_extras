using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Client.WebApi.Models
{
    public class WealthbagPortfolio
    { 
        public class ResponseBaseModelWb<T>
        { 
            public T Datas { get; set; }
        }

        public class PortfolioData
        {
            public int totalQuantity { get; set; }
            public List<PortfolioDetails> wealthBagStocks { get; set; }
        }

        public class WealthBagStocks
        {
            public string ClientCode { get; set; }
            public string CompanyName { get; set; }
            public int Quantity { get; set; }
            public string Symbol { get; set; }
            public string PortfolioName { get; set; }
        }

        public class WbPortfolioParam
        {
            [Required(ErrorMessage = "Client Code Is Required.")]
            public string ClientCode { get; set;}
        } 
         
        public class PortfolioDetails
        {
            public string CompanyName { get; set; }
            public int Quantity { get; set; }
            public string Symbol { get; set; }
            public string PortfolioName { get; set; }
        }
    }
}
