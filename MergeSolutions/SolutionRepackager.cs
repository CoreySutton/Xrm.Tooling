using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using CoreySutton.Utilities;
using CoreySutton.Xrm.Tooling.Core;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace CoreySutton.Xrm.Tooling.MergeSolutions
{
    public class SolutionRepackager
    {
        private const string _solutionPackagerPath = "../coretools/SolutionPackager.exe";
        private const string _workingDirectory = "./";
        private readonly IOrganizationService _sourceOrganizationService;
        private readonly IOrganizationService _targetOrganizationService;
        private readonly List<Entity> _allSourceSolutions = new List<Entity>();
        private readonly List<Entity> _sourceSolutions = new List<Entity>();
        private string _targetSolutionFileName;
        private string _targetSolutionName;
        private string _targetSolutionVersion;
        private Entity _targetSolution;

        public SolutionRepackager(
            IOrganizationService sourceOrganizationService,
            IOrganizationService targetOrganizationService)
        {
            _sourceOrganizationService = sourceOrganizationService;
            _targetOrganizationService = targetOrganizationService;

            _allSourceSolutions = SolutionUtil.GetUnmanagedSolutions(_sourceOrganizationService);
        }

        public void SetTargetSolution(string uniqueName = null)
        {
            if (string.IsNullOrEmpty(uniqueName))
            {
                SolutionUtil.PrintSolutions(_allSourceSolutions, true);
                _targetSolution = SolutionUtil.PromptPickSolution(_allSourceSolutions);
            }
            else
            {
                _targetSolution = SolutionUtil.GetSolutionByName(_sourceOrganizationService, uniqueName);
            }

            if (_targetSolution != null)
            {
                _targetSolutionName = _targetSolution.GetAttributeValue<string>("uniquename");
                _targetSolutionFileName = $"{_targetSolutionName}.zip";
            }
            else
            {
                _targetSolutionName = PromptTargetSolutionName();
                _targetSolutionFileName = $"{_targetSolutionName}.zip";
            }
        }

        public void SetSourceSolutions()
        {
            SolutionUtil.PrintSolutions(_allSourceSolutions);

            bool anotherSolution;
            do
            {
                Entity solution = SolutionUtil.PromptPickSolution(_allSourceSolutions);
                if (solution != null)
                {
                    _sourceSolutions.Add(solution);
                }

                anotherSolution = PromptAnotherSolution();
            } while (anotherSolution);
        }

        public void SetVersion()
        {
            if (_targetSolution != null)
            {
                string currentVersion = _targetSolution.GetAttributeValue<string>("version");
                _targetSolutionVersion = VersionNumberUtil.PromptIncrement(currentVersion);
            }
            else
            {
                _targetSolutionVersion = VersionNumberUtil.Prompt();
            }
        }

        public void StartMerge()
        {
            foreach (Entity solution in _sourceSolutions)
            {
                string solutionName = solution.GetAttributeValue<string>("uniquename");
                string solutionFileName = $"{solutionName}.zip";

                ExportUnmanagedSolution(solutionName, solutionFileName);
                UnpackUnmangedSolutionZip(solutionName, solutionFileName, _solutionPackagerPath, _workingDirectory);
                SetPackageName(_targetSolutionName, solutionName);
                SetPackageVersion(_targetSolutionVersion, solutionName);
                PackUnmangedSolutionZip(solutionName, _targetSolutionFileName, _solutionPackagerPath, _workingDirectory);
                ImportUnmanagedSolution(_targetSolutionFileName);

                // TODO
                // Clean up local environment
            }
        }

        private static string PromptTargetSolutionName()
        {
            Console.Write(">> Target solution name: ");
            while (true)
            {
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

        private bool PromptAnotherSolution()
        {
            Console.Write(">> Add another solution (yes|no)?: ");
            while (true)
            {
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

        private void ExportUnmanagedSolution(string solutionUniqueName, string filePath)
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

            ExportSolutionResponse response = (ExportSolutionResponse)_sourceOrganizationService.Execute(request);

            using (var fs = File.Create(filePath))
            {
                fs.Write(response.ExportSolutionFile, 0, response.ExportSolutionFile.Length);
            }

            ExConsole.WriteLineToRight("[Done]");
        }

        private void UnpackUnmangedSolutionZip(
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

        private void RunPackager(string binPath, string workingFolder, string parameters)
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

        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null) Console.WriteLine(e.Data.Replace("{", "{{").Replace("}", "}}"));
        }

        private void SetPackageName(string packageName, string packagePath)
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

        private void SetPackageVersion(string version, string packagePath)
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

            XmlElement versionElement = solutionManifest["Version"];
            if (versionElement == null)
            {
                Console.WriteLine("Could not find xml element <Version>");
                return;
            }

            versionElement.InnerText = version;

            solutionXml.Save(solutionXmlPath);
        }

        private void PackUnmangedSolutionZip(
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

        private void ImportUnmanagedSolution(string solutionPath)
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

            ImportSolutionResponse response = (ImportSolutionResponse)_targetOrganizationService.Execute(request);

            ExConsole.WriteLineToRight("[Done]");
        }
    }
}
