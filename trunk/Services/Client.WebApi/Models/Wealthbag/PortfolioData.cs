using System.Collections.Generic;

namespace Client.WebApi
{
    public class PortfolioData
    {
        public int totalQuantity { get; set; }
        public List<PortfolioDetails> wealthBagStocks { get; set; }
    }
}
