using System;
using System.Collections.Generic;
using System.Linq;
using CoreySutton.Utilities;
using CoreySutton.Xrm.Tooling.Core;
using CoreySutton.Xrm.Utilities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace CoreySutton.Xrm.Tooling.BulkExport
{
    internal class Program
    {
        private static void Main()
        {
            Config config = ConfigParser<Config>.Read("Config.json");
            OrganizationServiceProxy organizationService = CrmConnectorUtil.Connect(config.ConnectionString) as OrganizationServiceProxy;
            
            if (organizationService != null)
            {                
                IList<string> solutionUniqueNames = config.Solutions;

                if (config.TimeoutMinutes > 0)
                {
                    organizationService.Timeout = new TimeSpan(0, config.TimeoutMinutes, 0);
                }

                if (Validator.IsNullOrEmpty(solutionUniqueNames))
                {
                    ExConsole.WriteLine("No solutions found in Config.json, backing up all");
                    solutionUniqueNames = GetAllUnmanagedSolutions(organizationService);
                    if (config.ExcludeDefault)
                    {
                        solutionUniqueNames = solutionUniqueNames.Where(n => n.ToLower() != "default").ToList();
                    }
                }

                ExConsole.WriteLine($"Discovered {solutionUniqueNames.Count} solutions");

                new SolutionExport(organizationService).ExportMultiple(solutionUniqueNames, config.OutputPath, config.OutputFolderDateFormat);
            }

            ExConsole.WriteColor("Complete", ConsoleColor.Green);
            if (config.PromptBeforeClose)
            {
                Console.ReadLine();
            }
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
