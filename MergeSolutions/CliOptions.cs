using System.Collections.Generic;
using CommandLine;

namespace CoreySutton.Xrm.Tooling.MergeSolutions
{
    public class CliOptions
    {
        [Option('s', "solutions", HelpText = "List of solution unique names to merge")]
        public IEnumerable<string> Solutions { get; set; }

        [Option('t', "target", HelpText = "Unique name of solution to merge into")]
        public string Target { get; set; }

        [Option('f', "from", HelpText = "Connection string for CRM org to retrieve solutions from")]
        public string SourceConnectinString { get; set; }

        [Option('t', "to", HelpText = "Connection string for CRM org to merge solutions to")]
        public string TargetConnectionString { get; set; }
    }
}
