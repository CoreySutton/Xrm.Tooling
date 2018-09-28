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
            IOrganizationService sourceOrganizationService = ConnectToSource(options.SourceConnectinString);
            IOrganizationService targetOrganizationService = ConnectToTarget(options.TargetConnectionString) ?? sourceOrganizationService;

            SolutionRepackager solutionRepackager = new SolutionRepackager(sourceOrganizationService, targetOrganizationService);
            solutionRepackager.SetTargetSolution(options.Target);
            solutionRepackager.SetSourceSolutions(options.Solutions);
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

        private static IOrganizationService ConnectToSource(string connectionString)
        {
            string crmConnectionString = Properties.Settings.Default.SourceCrmConnectionString;
            return CrmConnectorUtil.Connect(connectionString ?? crmConnectionString);
        }

        private static IOrganizationService ConnectToTarget(string connectionString)
        {
            string crmConnectionString = Properties.Settings.Default.TargetCrmConnectionString;
            if (string.IsNullOrEmpty(crmConnectionString) && string.IsNullOrEmpty(connectionString))
            {
                return null;
            }
            return CrmConnectorUtil.Connect(connectionString ?? crmConnectionString);
        }
    }
}
