using System.Collections.Generic;

namespace Client.WebApi
{
    public class WBPortfolioDataStore
    {
        private static WBPortfolioDataStore reference = new WBPortfolioDataStore();
        private WBPortfolioDataStore()
        {

        }

        public static WBPortfolioDataStore Reference
        {
            get
            {
                return reference;
            }
        }
        public HashSet<WealthBagStocks> WealthBagStocks = new HashSet<WealthBagStocks>();

        public Dictionary<string, List<PortfolioDetails>> PortfolioDictionary { get; set; }

        public long TotalRows;
    }
}
