using System;
using System.Collections.Generic;
using System.Linq;
using CoreySutton.Utilities;
using CoreySutton.Xrm.Tooling.Core;
using CoreySutton.Xrm.Utilities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CoreySutton.Xrm.Tooling.BulkExport
{
    internal class Program
    {
        private static void Main()
        {
            string crmConnectionString = Properties.Settings.Default.CrmConnectionString;
            IOrganizationService organizationService = CrmConnectorUtil.Connect(crmConnectionString);
            if (organizationService != null)
            {
                Config config = ConfigParser<Config>.Read("Config.json");
                IList<string> solutionUniqueNames = config.Solutions;

                if (Validator.IsNullOrEmpty(solutionUniqueNames))
                {
                    ExConsole.WriteLine("No solutions found, backing up all");
                    solutionUniqueNames = GetAllUnmanagedSolutions(organizationService);
                }

                ExConsole.WriteLine($"Discovered {solutionUniqueNames.Count} solutions");

                new SolutionExport(organizationService).ExportMultiple(solutionUniqueNames);
            }

            ExConsole.WriteColor("Complete", ConsoleColor.Green);
            Console.ReadLine();
        }

        private static IList<string> GetAllUnmanagedSolutions(IOrganizationService organizationService)
        {
            var query = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("ismanaged", ConditionOperator.Equal, false),
                        new ConditionExpression("isvisible", ConditionOperator.Equal, true),
                    }
                },
                Orders =
                {
                    new OrderExpression("uniquename", OrderType.Ascending)
                }
            };

            EntityCollection results = organizationService.RetrieveMultiple(query);

            if (XrmValidator.IsNullOrEmpty(results)) return null;

            return results.Entities.Select(e => e.GetAttributeValue<string>("uniquename")).ToList();
        }
    }
}
