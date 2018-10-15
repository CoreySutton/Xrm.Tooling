using System;
using System.IO;
using Newtonsoft.Json;

namespace CoreySutton.Xrm.Tooling.Core
{
    public static class ConfigParser<TConfig>
    {
        public static TConfig Read(string path)
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<TConfig>(json);
        }

        public static bool Validate(TConfig config)
        {
            throw new NotImplementedException();
        }
    }
}
