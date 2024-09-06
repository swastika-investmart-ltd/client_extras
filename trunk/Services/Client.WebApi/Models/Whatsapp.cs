using System;
using System.Collections.Generic;

namespace Client.WebApi
{
    public class Whatsapp
    {
        public string token { get; set; }
        public string priority { get; set; }
        public string application { get; set; }
        public string template_id { get; set; }
        public string interactive_type { get; set; }
        public Array data { get; set; }
    }

    public class InputData
    {
        public string number { get; set; }
        public string message { get; set; }
        public List<string> template_message { get; set; }
    }

    public class CommunicationRequest
    {
        public string MobileNumber { get; set; }
        public string ClientId { get; set; }
        public string Message { get; set; }
        public string LTP { get; set; }
    }
}
