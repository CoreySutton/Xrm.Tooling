using CoreySutton.Xrm.Tooling.Core;
using CoreySutton.Xrm.Utilities;
using Microsoft.Xrm.Sdk;

namespace CoreySutton.Xrm.Tooling.BulkExport
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string crmConnectionString = Properties.Settings.Default.CrmConnectionString;
            IOrganizationService organizationService = CrmConnectorUtil.Connect(crmConnectionString);

            Config config = ConfigParser<Config>.Read("Config.json");

            var solutionExport = new SolutionExport(organizationService);
            foreach (string uniqueName in config.Solutions)
            {
                solutionExport.ExportUnmanaged(uniqueName, $"{uniqueName}.zip");
            }
        }
    }
}
