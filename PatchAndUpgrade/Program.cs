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
            // Parse current version number
            string[] currentVersionComponents = solution.GetAttributeValue<string>("version").Split('.');
            int.TryParse(currentVersionComponents[0], out int currentMajor);
            int.TryParse(currentVersionComponents[1], out int currentMinor);
            int.TryParse(currentVersionComponents[2], out int currentPatch);
            int.TryParse(currentVersionComponents[3], out int currentBuild);

            // Prompt for version number
            string version = string.Empty;
            while (string.IsNullOrEmpty(version))
            {
                Console.WriteLine("Set version number:");
                Console.Write(">> ");

                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Version number cannot be empty");
                }
                else
                {
                    string[] versionComponents = input.Split('.');
                    if (versionComponents.Length != 4 ||
                        !int.TryParse(versionComponents[0], out int major) ||
                        !int.TryParse(versionComponents[1], out int minor) ||
                        !int.TryParse(versionComponents[2], out int patch) ||
                        !int.TryParse(versionComponents[3], out int build))
                    {
                        Console.WriteLine("Invalid version number");
                    }
                    else if (major != currentMajor || minor != currentMinor)
                    {
                        Console.WriteLine("Cannot increment major or minor version numbers");
                    }
                    else if (patch < currentPatch && build <= currentBuild)
                    {
                        Console.WriteLine("Patch or build version numbers must be incremented");
                    }
                    else
                    {
                        version = input;
                    }
                }
            }


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
            // Parse current version number
            string[] currentVersionComponents = solution.GetAttributeValue<string>("version").Split('.');
            int.TryParse(currentVersionComponents[0], out int currentMajor);
            int.TryParse(currentVersionComponents[1], out int currentMinor);
            int.TryParse(currentVersionComponents[2], out int currentPatch);
            int.TryParse(currentVersionComponents[3], out int currentBuild);

            // Prompt for version number
            string version = string.Empty;
            while (string.IsNullOrEmpty(version))
            {
                Console.WriteLine("Set version number:");
                Console.Write(">> ");

                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Version number cannot be empty");
                }
                else
                {
                    string[] versionComponents = input.Split('.');
                    if (versionComponents.Length != 4 ||
                        !int.TryParse(versionComponents[0], out int major) ||
                        !int.TryParse(versionComponents[1], out int minor) ||
                        !int.TryParse(versionComponents[2], out int patch) ||
                        !int.TryParse(versionComponents[3], out int build))
                    {
                        Console.WriteLine("Invalid version number");
                    }
                    else if (patch != currentPatch || build != currentBuild)
                    {
                        Console.WriteLine("Cannot increment patch or build version numbers");
                    }
                    else if (major < currentMajor && minor <= currentMinor)
                    {
                        Console.WriteLine("Major or minor version numbers must be incremented");
                    }
                    else
                    {
                        version = input;
                    }
                }
            }


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
