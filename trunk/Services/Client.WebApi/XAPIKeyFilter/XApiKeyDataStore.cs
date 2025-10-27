using System.Collections.Generic;

namespace Client.WebApi
{
    public class XApiKeyDataStore
    {
        private static XApiKeyDataStore reference = new XApiKeyDataStore();

        private XApiKeyDataStore()
        {
            xapikeysDictionary = new Dictionary<string, string>();
        }

        public static XApiKeyDataStore Reference
        {
            get
            {
                return reference;
            }
        }
        public Dictionary<string, string> xapikeysDictionary { get; set; }
    }
}
