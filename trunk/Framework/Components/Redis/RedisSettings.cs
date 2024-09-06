using System;
using System.Collections.Generic;
using System.Text;

namespace Components
{
    public class RedisSettings
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string InstanceName { get; set; }
        public bool AllowAdmin { get; set; }
        public bool Ssl { get; set; }
        public int ConnectTimeout { get; set; }
        public int ConnectRetry { get; set; }
        public int Database { get; set; }
    }
}
