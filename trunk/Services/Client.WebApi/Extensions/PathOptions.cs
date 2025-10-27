using System.Collections.Generic;
using System;

namespace Client.WebApi
{
    public class PathOptions
    {
        public HashSet<string> Paths { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}
