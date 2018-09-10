using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using CoreySutton.Utilities;
using CoreySutton.Xrm.Tooling.Core;
using CoreySutton.Xrm.Utilities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace CoreySutton.Xrm.Tooling.MergeSolutions
{
    internal class Program
    {
        // TODO copy coretools to bin as this relative path will break
        private const string _solutionPackagerPath = "../coretools/SolutionPackager.exe";
        private const string _workingDirectory = "./";

        private static void Main(string[] args)
        {
            try
            {
                IOrganizationService sourceOrganizationService = ConnectToSource();
                IOrganizationService targetOrganizationService = ConnectToTarget() ?? sourceOrganizationService;

                string targetSolutionName = PromptTargetSolutionName();

                bool anotherSolution;
                do
                {
                    AddSolutionToTarget(sourceOrganizationService, targetOrganizationService, targetSolutionName);
                    anotherSolution = PromptAnotherSolution();
                } while (anotherSolution);

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

        private static string PromptTargetSolutionName()
        {
            Console.WriteLine("Target solution name:");
            while (true)
            {
                Console.Write(">> ");
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Invalid option. Try again");
                }
                else
                {
                    return input;
                }
            }
        }

        private static void AddSolutionToTarget(
            IOrganizationService sourceOrganizationService,
            IOrganizationService targetOrganizationService,
            string targetSolutionName)
        {
            // Get solution
            Entity solution = SolutionUtil.GetSolution(sourceOrganizationService);
            if (solution == null) return;

            string solutionName = solution.GetAttributeValue<string>("uniquename");
            string solutionFileName = $"{solutionName}.zip";
            string targetSolutionFileName = $"{targetSolutionName}.zip";

            // Download solution
            ExportUnmanagedSolution(sourceOrganizationService, solutionName, solutionFileName);

            // Unpack solution zip
            UnpackUnmangedSolutionZip(solutionName, solutionFileName, _solutionPackagerPath, _workingDirectory);

            // Modify unique name
            SetPackageName(targetSolutionName, solutionName);

            // Pack solution zip
            PackUnmangedSolutionZip(solutionName, targetSolutionFileName, _solutionPackagerPath, _workingDirectory);

            // Upload solution to CRM
            ImportUnmanagedSolution(targetOrganizationService, targetSolutionFileName);

            // TODO
            // Clean up local environment
        }

        private static void ExportUnmanagedSolution(
            IOrganizationService organizationService,
            string solutionUniqueName,
            string filePath)
        {
            var request = new ExportSolutionRequest
            {
                SolutionName = solutionUniqueName,
                ExportAutoNumberingSettings = false,
                ExportCalendarSettings = false,
                ExportCustomizationSettings = false,
                ExportEmailTrackingSettings = false,
                ExportExternalApplications = false,
                ExportGeneralSettings = false,
                ExportIsvConfig = false,
                ExportMarketingSettings = false,
                ExportOutlookSynchronizationSettings = false,
                ExportRelationshipRoles = false,
                ExportSales = false,
                Managed = false
            };

            ExConsole.WriteLine($"Exporting {solutionUniqueName}...");

            ExportSolutionResponse response = (ExportSolutionResponse)organizationService.Execute(request);

            // Save solution 
            using (var fs = File.Create(filePath))
            {
                fs.Write(response.ExportSolutionFile, 0, response.ExportSolutionFile.Length);
            }

            ExConsole.WriteLineToRight("[Done]");
        }

        private static void UnpackUnmangedSolutionZip(
            string unpackTo,
            string unpackFrom,
            string binPath,
            string binFolder)
        {
            // Run CrmSvcUtil 
            string parameters = "/action:Extract" +
                                $" /zipfile:\"{unpackFrom}\"" +
                                $" /folder:\"{unpackTo}\"" +
                                " /packagetype:\"unmanaged\"" +
                                " /allowWrite:Yes" +
                                " /allowDelete:Yes" +
                                " /clobber" +
                                " /errorlevel:Verbose" +
                                " /nologo" +
                                " /log:packagerlog.txt";

            RunPackager(binPath, binFolder, parameters);
        }

        private static void RunPackager(string binPath, string workingFolder, string parameters)
        {
            var procStart = new ProcessStartInfo(binPath, parameters)
            {
                WorkingDirectory = workingFolder,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Normal
            };

            Console.WriteLine($"Running {binPath} {parameters}");

            Process proc = null;
            var exitCode = 0;
            try
            {
                proc = Process.Start(procStart);
                if (proc != null)
                {
                    proc.OutputDataReceived += Proc_OutputDataReceived;
                    proc.ErrorDataReceived += Proc_OutputDataReceived;
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    proc.WaitForExit(20 * 60 * 60 * 1000);
                    proc.CancelOutputRead();
                    proc.CancelErrorRead();
                }
            }
            finally
            {
                if (proc != null)
                {
                    exitCode = proc.ExitCode;
                    proc.Close();
                }
            }
            if (exitCode != 0)
            {
                throw new Exception($"Solution Packager exited with error {exitCode}");
            }
        }

        private static void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null) Console.WriteLine(e.Data.Replace("{", "{{").Replace("}", "}}"));
        }

        private static void SetPackageName(string packageName, string packagePath)
        {
            string solutionXmlPath = $@"{packagePath}\Other\Solution.xml";

            XmlDocument solutionXml = new XmlDocument();
            solutionXml.Load(solutionXmlPath);

            XmlElement importExportXml = solutionXml["ImportExportXml"];
            if (importExportXml == null)
            {
                Console.WriteLine("Could not find xml element <ImportExportXml>");
                return;
            }

            XmlElement solutionManifest = importExportXml["SolutionManifest"];
            if (solutionManifest == null)
            {
                Console.WriteLine("Could not find xml element <SolutionManifest>");
                return;
            }

            XmlElement uniqueName = solutionManifest["UniqueName"];
            if (uniqueName == null)
            {
                Console.WriteLine("Could not find xml element <UniqueName>");
                return;
            }

            XmlElement localizedNames = solutionManifest["LocalizedNames"];
            if (localizedNames == null)
            {
                Console.WriteLine("Could not find xml element <LocalizedNames>");
                return;
            }

            XmlElement localizedName = localizedNames["LocalizedName"];
            if (localizedName == null)
            {
                Console.WriteLine("Could not find xml element <LocalizedName>");
                return;
            }

            uniqueName.InnerText = packageName;
            localizedName.Attributes["description"].Value = packageName;

            solutionXml.Save(solutionXmlPath);
        }

        private static void PackUnmangedSolutionZip(
            string packFrom,
            string packTo,
            string binPath,
            string binFolder)
        {
            // Run CrmSvcUtil 
            string parameters = "/action:Pack" +
                                $" /zipfile:\"{packTo}\"" +
                                $" /folder:\"{packFrom}\"" +
                                " /packagetype:\"unmanaged\"" +
                                " /allowWrite:Yes" +
                                " /allowDelete:Yes" +
                                " /clobber" +
                                " /errorlevel:Verbose" +
                                " /nologo" +
                                " /log:packagerlog.txt";

            RunPackager(binPath, binFolder, parameters);
        }

        private static void ImportUnmanagedSolution(
            IOrganizationService organizationService,
            string solutionPath)
        {
            byte[] solutionBytes = File.ReadAllBytes(solutionPath);

            var request = new ImportSolutionRequest
            {
                OverwriteUnmanagedCustomizations = true,
                PublishWorkflows = true,
                CustomizationFile = solutionBytes,
                ImportJobId = Guid.NewGuid()
            };

            ExConsole.WriteLine("Importing solution...");

            ImportSolutionResponse response = (ImportSolutionResponse)organizationService.Execute(request);

            ExConsole.WriteLineToRight("[Done]");
        }

        private static bool PromptAnotherSolution()
        {
            Console.WriteLine("Add another solution? (yes/no)");
            while (true)
            {
                Console.Write(">> ");
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) || (input != "yes" && input != "no" && input != "y" && input != "n"))
                {
                    Console.WriteLine("Invalid option. Try again");
                }
                else if (input == "yes" || input == "y")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
