using System;
using System.IO;
using Newtonsoft.Json;

namespace CoreySutton.Xrm.Tooling.MergeSolutions
{
    public static class ConfigParser
    {
        public static Config Read(string path)
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Config>(json);
        }

        public static bool Validate(Config config)
        {
            throw new NotImplementedException();
        }
    }
}
