using System;
using System.ServiceModel;
using CoreySutton.Utilities;
using CoreySutton.Xrm.Utilities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace CoreySutton.Xrm.Tooling.TransportSolution
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            try
            {
                Run();
            }
            catch (Exception ex)
            {
                ExConsole.WriteLineColor(ex.Message, ConsoleColor.DarkRed);
                ExConsole.WriteLineColor(ex.StackTrace, ConsoleColor.DarkGray);
            }

            Console.WriteLine("Complete");
            Console.ReadLine();
        }

        private static void Run()
        {
            IOrganizationService sourceOrg = CrmConnectorUtil.Connect(Properties.Settings.Default.SourceCrmConnectionString);
            IOrganizationService targetOrg = CrmConnectorUtil.Connect(Properties.Settings.Default.TargetCrmConnectionString);

            string uniqueName = Prompt("Solution Unique Name", "Cannot be empty");

            Console.Write("Exporting...");
            ExportSolutionResponse exportRespone = ExportSolution(uniqueName, sourceOrg);
            byte[] solutionBytes = exportRespone.ExportSolutionFile;
            ExConsole.WriteLineToRight("[Done]");

            Console.Write("Importing...");
            ImportSolutionResponse importResponse = ImportSolution(solutionBytes, targetOrg);
            ExConsole.WriteLineToRight("[Done]");

            if (importResponse != null)
            {
                Console.Write("Publishing...");
                Publish(targetOrg);
                ExConsole.WriteLineToRight("[Done]");
            }
        }

        private static string Prompt(string question, string emptyError)
        {
            while (true)
            {
                Console.Write($"{question}: ");
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine($"{emptyError}, please try again!");
                    Console.WriteLine();
                }
                else
                {
                    return input;
                }
            }
        }

        private static ExportSolutionResponse ExportSolution(string uniqueName, IOrganizationService organizationService)
        {
            var request = new ExportSolutionRequest
            {
                Managed = false,
                SolutionName = uniqueName
            };

            try
            {
                return (ExportSolutionResponse)organizationService.Execute(request);
            }
            catch (FaultException<OrganizationServiceFault>)
            {
                ExConsole.WriteLineColor($"Could not export solution {uniqueName}", ConsoleColor.Red);
                throw;
            }
        }

        private static ImportSolutionResponse ImportSolution(byte[] solutionBytes, IOrganizationService organizationService)
        {
            var request = new ImportSolutionRequest
            {
                CustomizationFile = solutionBytes,
                ImportJobId = Guid.NewGuid(),
                OverwriteUnmanagedCustomizations = false,
                PublishWorkflows = true
            };

            try
            {
                return (ImportSolutionResponse)organizationService.Execute(request);
            }
            catch (FaultException<OrganizationServiceFault>)
            {
                ExConsole.WriteLineColor("Could not import solution", ConsoleColor.Red);
                throw;
            }
        }

        private static void Publish(IOrganizationService organizationService)
        {
            PublishAllXmlRequest publishRequest = new PublishAllXmlRequest();

            try
            {
                organizationService.Execute(publishRequest);
            }
            catch (FaultException<OrganizationServiceFault>)
            {
                ExConsole.WriteLineColor("Could not publish solution", ConsoleColor.Red);
                throw;
            }
        }
    }
}
