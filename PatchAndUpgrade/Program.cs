using System;
using CoreySutton.Xrm.Tooling.Core;
using CoreySutton.Xrm.Utilities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace CoreySutton.Xrm.Tooling.PatchAndUpgrade
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Connect to CRM
            string crmConnectionString = Properties.Settings.Default.CrmConnectionString;
            IOrganizationService organizationService = CrmConnectorUtil.Connect(crmConnectionString);

            // Get solution
            Entity solution = SolutionUtil.GetSolution(organizationService);
            if (solution == null) return;

            // Execute action
            Action<IOrganizationService, Entity> action = PromptForAction();
            action.Invoke(organizationService, solution);

            Console.WriteLine("Complete");
            Console.ReadLine();
        }

        private static Action<IOrganizationService, Entity> PromptForAction()
        {
            Console.WriteLine("Select an action:");
            Console.WriteLine("(0) Patch");
            Console.WriteLine("(1) Upgrade");
            Console.Write(">> ");

            int actionNumber = -1;
            while (actionNumber < 0)
            {
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input, out actionNumber))
                {
                    Console.WriteLine("Invalid selection");
                }
                else if (actionNumber < 0 || actionNumber > 1)
                {
                    Console.WriteLine("Selection out of range");
                }
            }

            switch (actionNumber)
            {
                case 0:
                    return CloneAsPatch;
                case 1:
                    return CloneAsUpgrade;
                default:
                    throw new Exception("Action selection out of range");
            }
        }

        private static void CloneAsPatch(IOrganizationService organizationService, Entity solution)
        {
            string currentVersion = solution.GetAttributeValue<string>("version");
            string version = VersionNumberUtil.PromptIncrementPatchOrBuild(currentVersion);

            // Create patch
            CloneAsPatchRequest cloneRequest = new CloneAsPatchRequest();
            cloneRequest.DisplayName = $"{solution.GetAttributeValue<string>("friendlyname")} v{version}";
            cloneRequest.ParentSolutionUniqueName = solution.GetAttributeValue<string>("uniquename");
            cloneRequest.VersionNumber = version;
            CloneAsPatchResponse cloneResponse = (CloneAsPatchResponse)organizationService.Execute(cloneRequest);

            Console.WriteLine($"Created patch solution {cloneRequest.DisplayName}");
        }

        private static void CloneAsUpgrade(IOrganizationService organizationService, Entity solution)
        {
            string currentVersion = solution.GetAttributeValue<string>("version");
            string version = VersionNumberUtil.PromptIncrementMajorOrMinor(currentVersion);

            // Create patch
            CloneAsSolutionRequest cloneRequest = new CloneAsSolutionRequest();
            cloneRequest.DisplayName = solution.GetAttributeValue<string>("friendlyname");
            cloneRequest.ParentSolutionUniqueName = solution.GetAttributeValue<string>("uniquename");
            cloneRequest.VersionNumber = version;
            CloneAsSolutionResponse cloneResponse = (CloneAsSolutionResponse)organizationService.Execute(cloneRequest);

            Console.WriteLine($"Upgraded solution {cloneRequest.DisplayName}");
        }
    }
}
