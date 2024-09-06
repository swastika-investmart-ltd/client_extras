using System.Collections.Generic;

namespace Client.WebApi
{
    public class MCXDataStore
    {
        private static MCXDataStore reference = new MCXDataStore();

        private MCXDataStore()
        {

        }
        public static MCXDataStore Reference
        {
            get
            {
                return reference;
            }
        }
        public List<MCXUnderlyingInfoResponse> MCXDataStoreMdl { get; set; }
    }
}