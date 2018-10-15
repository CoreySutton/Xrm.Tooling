using System;
using System.Collections.Generic;
using CoreySutton.Utilities;
using CoreySutton.Xrm.Tooling.Core;
using CoreySutton.Xrm.Utilities;
using Microsoft.Xrm.Sdk;

namespace CoreySutton.Xrm.Tooling.BulkExport
{
    internal class Program
    {
        private static void Main()
        {
            string crmConnectionString = Properties.Settings.Default.CrmConnectionString;
            IOrganizationService organizationService = CrmConnectorUtil.Connect(crmConnectionString);

            Config config = ConfigParser<Config>.Read("Config.json");
            IList<string> solutionUniqueNames = config.Solutions;

            if (Validator.IsNullOrEmpty(solutionUniqueNames))
            {
                ExConsole.WriteLine("No solutions found, exiting...");
            }
            else
            {
                ExConsole.WriteLine($"Discovered {solutionUniqueNames.Count} solutions");
                ExportSolutions(solutionUniqueNames, organizationService);
            }

            ExConsole.WriteColor("Complete", ConsoleColor.Green);
            Console.ReadLine();
        }

        private static void ExportSolutions(IList<string> solutionUniqueNames, IOrganizationService organizationService)
        {
            var solutionExport = new SolutionExport(organizationService);
            foreach (string uniqueName in solutionUniqueNames)
            {
                ExConsole.Write($"Exporting {uniqueName}");
                try
                {
                    solutionExport.ExportUnmanaged(uniqueName, $"{uniqueName}.zip");
                    ExConsole.WriteLineToRight("[Done]");
                }
                catch (Exception ex)
                {
                    ExConsole.WriteLineToRight("[Failed]");
                    ExConsole.WriteColor($"An exception occurred: {ex.Message}", ConsoleColor.Red);
                    ExConsole.WriteColor(ex.StackTrace, ConsoleColor.DarkGray);
                }
            }
        }
    }
}
