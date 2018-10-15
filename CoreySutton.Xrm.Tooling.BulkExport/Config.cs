using System.Collections.Generic;
using Newtonsoft.Json;

namespace CoreySutton.Xrm.Tooling.BulkExport
{
    public class Config
    {
        [JsonProperty("Solutions")]
        public IList<string> Solutions { get; set; }
    }
}
