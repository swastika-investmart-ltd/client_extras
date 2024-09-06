using System.Collections.Generic;

namespace Client.WebApi
{
    public class BSESecurityReportDataStore
    {
        private static BSESecurityReportDataStore reference = new BSESecurityReportDataStore();
        private BSESecurityReportDataStore()
        {

        }

        public static BSESecurityReportDataStore Reference
        {
            get
            {
                return reference;
            }
        }
        public List<BSESecurityReqMdl> BSESecurityReqMdl { get; set; }
    }
}
