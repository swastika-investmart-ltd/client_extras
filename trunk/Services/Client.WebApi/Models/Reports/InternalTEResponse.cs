using System.Collections.Generic;

namespace Client.WebApi
{
    public class InternalTEResponse
    {
        public List<string> COLUMNS { get; set; }
        public List<List<string>> DATA { get; set; }
    }
}
