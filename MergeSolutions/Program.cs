using System;
using System.Collections.Generic;
using CommandLine;
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
                Parser.Default.ParseArguments<CliOptions>(args)
                    .WithParsed(Run)
                    .WithNotParsed(HandleParseError);
            }
            catch (Exception ex)
            {
                ExConsole.WriteColor($"An exception occurred: {ex.Message}", ConsoleColor.Red);
                ExConsole.WriteColor(ex.StackTrace, ConsoleColor.DarkGray);
            }

            Console.WriteLine("Complete");
            Console.ReadLine();
        }

        private static void Run(CliOptions options)
        {
            IOrganizationService sourceOrganizationService = ConnectToSource();
            IOrganizationService targetOrganizationService = ConnectToTarget() ?? sourceOrganizationService;

            SolutionRepackager solutionRepackager = new SolutionRepackager(sourceOrganizationService, targetOrganizationService);
            solutionRepackager.SetTargetSolution(options.Target);
            solutionRepackager.SetSourceSolutions();
            solutionRepackager.SetVersion();
            solutionRepackager.StartMerge();
        }

        private static void HandleParseError(IEnumerable<Error> errors)
        {
            foreach (Error error in errors)
            {
                Console.WriteLine(error);
                Console.WriteLine($"Tag: {error.Tag}");
                Console.WriteLine($"Stops Processing: {error.StopsProcessing}");
            }
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
