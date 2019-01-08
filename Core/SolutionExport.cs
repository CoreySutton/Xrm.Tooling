using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using CoreySutton.Utilities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace CoreySutton.Xrm.Tooling.Core
{
    public class SolutionExport
    {
        public bool AutoNumbering = false;
        public bool Calendar = false;
        public bool Customization = false;
        public bool EmailTracking = false;
        public bool ExternalApplications = false;
        public bool General = false;
        public bool Isv = false;
        public bool Marketing = false;
        public bool OutlookSynchronization = false;
        public bool RelationshipRoles = false;
        public bool Sales = false;
        private readonly IOrganizationService _organizationService;
        private readonly DateTime _outputFolderDate;
        private const string _defaultOutputFolderDateFormat = "yyyyMMdd-HHmmss";

        public SolutionExport(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
            _outputFolderDate = DateTime.Now;
        }

        public void Export(
            string solutionUniqueName, 
            string filePath = "", 
            string outputFolderDateFormat = _defaultOutputFolderDateFormat, 
            bool managed = false)
        {
            var request = new ExportSolutionRequest
            {
                SolutionName = solutionUniqueName,
                ExportAutoNumberingSettings = AutoNumbering,
                ExportCalendarSettings = Calendar,
                ExportCustomizationSettings = Customization,
                ExportEmailTrackingSettings = EmailTracking,
                ExportExternalApplications = ExternalApplications,
                ExportGeneralSettings = General,
                ExportIsvConfig = Isv,
                ExportMarketingSettings = Marketing,
                ExportOutlookSynchronizationSettings = OutlookSynchronization,
                ExportRelationshipRoles = RelationshipRoles,
                ExportSales = Sales,
                Managed = managed
            };

            ExConsole.WriteLine($"Exporting {solutionUniqueName}...");

            ExportSolutionResponse response;
            try
            {
                response = (ExportSolutionResponse)_organizationService.Execute(request);
            }
            catch (FaultException<OrganizationServiceFault>)
            {
                ExConsole.WriteLineColor($"Failed to export solution {solutionUniqueName}", ConsoleColor.Red);
                throw;
            }

            // Create directory if it doesnt already exist
            string dateFormat = string.IsNullOrEmpty(outputFolderDateFormat) ? _defaultOutputFolderDateFormat : outputFolderDateFormat;
            string path = $"{filePath}\\{_outputFolderDate.ToString(dateFormat)}";
            Directory.CreateDirectory(path);

            using (var fs = File.Create($"{path}\\{solutionUniqueName}.zip"))
            {
                fs.Write(response.ExportSolutionFile, 0, response.ExportSolutionFile.Length);
            }
        }

        public void ExportMultiple(
            IList<string> solutionUniqueNames, 
            string filePath = "",
            string outputFolderDateFormat = _defaultOutputFolderDateFormat, 
            bool managed = false)
        {
            foreach (string solutionUniqueName in solutionUniqueNames)
            {
                try
                {
                    Export(solutionUniqueName, filePath, outputFolderDateFormat, managed);
                }
                catch (Exception ex)
                {
                    ExConsole.WriteLineColor($"An exception occurred: {ex.Message}", ConsoleColor.Red);
                    ExConsole.WriteLineColor(ex.StackTrace, ConsoleColor.DarkGray);
                }
            }
        }

        public void ParallelExportMultiple(IList<string> solutionUniqueNames, string filePath = "", bool managed = false)
        {
            Parallel.ForEach(
                solutionUniqueNames,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                uniqueName => Export(uniqueName, filePath, managed:managed));
        }
    }
}
