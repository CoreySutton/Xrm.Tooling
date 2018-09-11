using System;
using CoreySutton.Utilities;
using CoreySutton.Xrm.Utilities;
using Microsoft.Xrm.Sdk;

namespace CoreySutton.Xrm.Tooling.MergeSolutions
{
    internal class Program
    {


        private static void Main(string[] args)
        {
            try
            {
                IOrganizationService sourceOrganizationService = ConnectToSource();
                IOrganizationService targetOrganizationService = ConnectToTarget() ?? sourceOrganizationService;

                SolutionRepackager solutionRepackager = new SolutionRepackager(sourceOrganizationService, targetOrganizationService);
                solutionRepackager.SetTargetSolution();
                solutionRepackager.SetSourceSolutions();
                solutionRepackager.StartMerge();
            }
            catch (Exception ex)
            {
                ExConsole.WriteColor($"An exception occurred: {ex.Message}", ConsoleColor.Red);
                ExConsole.WriteColor(ex.StackTrace, ConsoleColor.DarkGray);
            }

            Console.WriteLine("Complete");
            Console.ReadLine();
        }

        private static IOrganizationService ConnectToSource()
        {
            string crmConnectionString = Properties.Settings.Default.SourceCrmConnectionString;
            return CrmConnectorUtil.Connect(crmConnectionString);
        }

        private static IOrganizationService ConnectToTarget()
        {
            string crmConnectionString = Properties.Settings.Default.TargetCrmConnectionString;
            return string.IsNullOrEmpty(crmConnectionString) ? null : CrmConnectorUtil.Connect(crmConnectionString);
        }
    }
}
