using System;
using System.Collections.Generic;
using System.Text;

namespace Components
{
    public class RedisAdminAuthResponse
    {
        public string AccessKey { get; set; }
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiry { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
