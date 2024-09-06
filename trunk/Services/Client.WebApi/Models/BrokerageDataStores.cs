using Client.Models.WebApi;
using System.Collections.Generic;


namespace Client.Models.WebApi
{
    public class BrokerageDataStore
    {
        private static BrokerageDataStore reference = new BrokerageDataStore();
        private BrokerageDataStore()
        {
            topTwoEarners = new List<TopTwoEarners>();
            totalReferredClient = new Dictionary<string, int>();
            //brkDictionary = new Dictionary<string, List<BrokerageDetails>>();
            threeMonthDictionary = new Dictionary<string, double>();
            thisMonthDictionary = new Dictionary<string, double>();
            lastMonthDictionary = new Dictionary<string, double>();
            directIndirectClients = new List<DirectCustomers>();
        }

        public static BrokerageDataStore Reference
        {
            get
            {
                return reference;
            }
        }

        //public List<BrokerageDetails> BrokerageDataJson { get; set; }

        public List<TopTwoEarners> topTwoEarners { get; set; }
        //public Dictionary<string, List<BrokerageDetails>> brkDictionary { get; set; }
        public Dictionary<string, int> totalReferredClient { get; set; }
        public Dictionary<string, double> threeMonthDictionary { get; set; }
        public Dictionary<string, double> thisMonthDictionary { get; set; }
        public Dictionary<string, double> lastMonthDictionary { get; set; }
        public List<DirectCustomers> directIndirectClients { get; set; }

    }
}

