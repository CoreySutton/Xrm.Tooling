using System.IO;
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

        public SolutionExport(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        public void ExportUnmanaged(string solutionUniqueName, string filePath)
        {
            Export(solutionUniqueName, filePath, false);
        }

        public void ExportManaged(string solutionUniqueName, string filePath)
        {
            Export(solutionUniqueName, filePath, true);
        }

        public void Export(string solutionUniqueName, string filePath, bool managed)
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

            ExportSolutionResponse response = (ExportSolutionResponse)_organizationService.Execute(request);

            using (var fs = File.Create(filePath))
            {
                fs.Write(response.ExportSolutionFile, 0, response.ExportSolutionFile.Length);
            }

            ExConsole.WriteLineToRight("[Done]");
        }
    }
}
