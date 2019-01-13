using System.Collections.Generic;
using Newtonsoft.Json;

namespace CoreySutton.Xrm.Tooling.MergeSolutions
{
    public class Config
    {
        [JsonProperty("Solutions")]
        public IList<string> Solutions { get; set; }

        [JsonProperty("SolutionName")]
        public string SolutionName { get; set; }

        [JsonProperty("Version")]
        public string Version { get; set; }
    }
}
