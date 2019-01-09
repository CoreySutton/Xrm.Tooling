using System.Collections.Generic;
using Newtonsoft.Json;

namespace CoreySutton.Xrm.Tooling.BulkExport
{
    public class Config
    {
        [JsonProperty("Solutions")]
        public IList<string> Solutions { get; set; }

        [JsonProperty("OutputPath")]
        public string OutputPath { get; set; }

        [JsonProperty("ExcludeDefault")]
        public bool ExcludeDefault { get; set; }

        [JsonProperty("OutputFolderDateFormat")]
        public string OutputFolderDateFormat { get; set; }

        [JsonProperty("TimeoutMinutes")]
        public int TimeoutMinutes { get; set; }
    }
}
