using System;
using System.IO;
using Newtonsoft.Json;

namespace CoreySutton.Xrm.Tooling.Core
{
    public static class ConfigParser<TConfig>
    {
        public static TConfig Read(string configFileName)
        {
            string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string json = File.ReadAllText($"{exePath}\\{configFileName}");
            return JsonConvert.DeserializeObject<TConfig>(json);
        }

        public static bool Validate(TConfig config)
        {
            throw new NotImplementedException();
        }
    }
}
