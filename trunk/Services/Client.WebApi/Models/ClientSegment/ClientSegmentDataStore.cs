using System.Collections.Generic;

namespace Client.WebApi.Models.ClientSegment
{
    public class ClientSegmentDataStore
    {
        private static ClientSegmentDataStore reference = new ClientSegmentDataStore();
        private ClientSegmentDataStore()
        {
            SegmentDict = new Dictionary<string, string>();
        }
        public static ClientSegmentDataStore Reference
        {
            get
            {
                return reference;
            }
        }
        public Dictionary<string, string> SegmentDict = new();
    }
}
