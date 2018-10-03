using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using CoreySutton.Utilities;
using CoreySutton.Xrm.Utilities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace CoreySutton.Xrm.Tooling.ConvertToGlobalOptionSet
{
    class Program
    {
        // Specify which language code to use. If you are using a language
        // other than US English, you will need to modify this value accordingly.
        // See http://msdn.microsoft.com/en-us/library/0h88fahh.aspx
        private const int _languageCode = 1033;
        private static IOrganizationService _organizationService;

        static void Main(string[] args)
        {
            // Connect to CRM
            _organizationService = CrmConnectorUtil.Connect(Properties.Settings.Default.CrmConnectionString);

            // Prompt for existing option set attribute name
            string entityLogicalName = PromptEntityLogicalName();
            string attributeLogicalName = PromptAttributeLogicalName();

            // Retrieve option set
            IList<OptionMetadata> optionMetadatas = RetrieveOptionSetAttributeMetadata(entityLogicalName, attributeLogicalName);
            if (!Validator.IsNullOrEmpty(optionMetadatas))
            {
                // Prompt for new global option set name
                string globalOptionSetLogicalName = PromptGlobalOptionSetLogicalName();
                string globalOptionSetDisplayName = PromptGlobalOptionSetDisplayName();

                // Create global option set
                if (!DoesGlobalOptionSetExist(globalOptionSetLogicalName))
                {
                    CreateGlobalOptionSet(globalOptionSetLogicalName, globalOptionSetDisplayName, optionMetadatas);
                }
            }

            Console.WriteLine("Complete");
            Console.ReadLine();
        }

        private static string PromptEntityLogicalName()
        {
            return Prompt("Entity Logical Name", "Cannot be empty");
        }

        private static string PromptAttributeLogicalName()
        {
            return Prompt("Attribute Logical Name", "Cannot be empty");
        }

        private static string PromptGlobalOptionSetLogicalName()
        {
            return Prompt("Global Option Set Logical Name", "Cannot be empty");
        }

        private static string PromptGlobalOptionSetDisplayName()
        {
            return Prompt("Global Option Set Display Name", "Cannot be empty");
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

        private static IList<OptionMetadata> RetrieveOptionSetAttributeMetadata(string entityLogicalName, string attributeLogicalName)
        {
            PicklistAttributeMetadata osvAttrMetadata =
                RetrieveAttributeMetadata<PicklistAttributeMetadata>(entityLogicalName, attributeLogicalName);

            return osvAttrMetadata?.OptionSet.Options.ToList();
        }

        private static TAttributeMetadata RetrieveAttributeMetadata<TAttributeMetadata>(string entityLogicalName, string attributeLogicalName)
            where TAttributeMetadata : EnumAttributeMetadata
        {
            var retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = attributeLogicalName,
                RetrieveAsIfPublished = true
            };

            try
            {
                var retrieveAttributeResponse =
                    (RetrieveAttributeResponse)_organizationService.Execute(retrieveAttributeRequest);

                return (TAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                ExConsole.WriteLineColor($"Could not find attribute {attributeLogicalName} for entity {entityLogicalName}: {ex.Message}", ConsoleColor.Red);
                return default(TAttributeMetadata);
            }
        }

        private static bool DoesGlobalOptionSetExist(string logicalName)
        {
            RetrieveOptionSetRequest request = new RetrieveOptionSetRequest
            {
                Name = logicalName
            };

            try
            {
                var respone = (RetrieveOptionSetResponse)_organizationService.Execute(request);
                if (respone?.OptionSetMetadata != null && respone.Results != null)
                {
                    return true;
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                ExConsole.WriteLineColor(
                    $"Could not find global option set {logicalName}: {ex.Message}",
                    ConsoleColor.DarkGray);
            }


            return false;
        }

        private static void CreateGlobalOptionSet(
            string logicalName,
            string displayName,
            IList<OptionMetadata> optionMetadata)
        {
            CreateOptionSetRequest createOptionSetRequest = new CreateOptionSetRequest
            {
                OptionSet = new OptionSetMetadata(new OptionMetadataCollection(optionMetadata))
                {
                    Name = logicalName,
                    DisplayName = new Label(displayName, _languageCode),
                    IsGlobal = true,
                    OptionSetType = OptionSetType.Picklist
                }
            };

            try
            {
                var optionsResp = (CreateOptionSetResponse)_organizationService.Execute(createOptionSetRequest);
                ExConsole.WriteLineColor(
                    $"Created global option set {displayName} ({logicalName}) with id {optionsResp.OptionSetId}",
                    ConsoleColor.Green);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                ExConsole.WriteLineColor(
                    $"Could not create global option set {logicalName}: {ex.Message}",
                    ConsoleColor.Red);
            }
        }
    }
}
