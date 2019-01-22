using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;

namespace CoreySutton.Xrm.Tooling.DependencyReport
{
    public class GlobalOptionSetDependecyReport
    {
        private readonly IOrganizationService orgSvc;

        public GlobalOptionSetDependecyReport(IOrganizationService orgSvc)
        {
            this.orgSvc = orgSvc;
        }

        public void Create()
        {
            string globalOptionSetName = PromptOptionSetName();

            // get global option set id
            Guid? globalOptionSetId = GlobalOptionSetDao.GetId(orgSvc, globalOptionSetName);
            if (globalOptionSetId == null) return;

            IEnumerable<Dependency> dependencies = GlobalOptionSetDao.GetDependencies(orgSvc, globalOptionSetId.Value);

            Console.WriteLine("");
            foreach (Dependency d in dependencies)
            {
                //Just testing for Attributes
                if (d.DependentComponentType == componenttype.Attribute)
                {
                    AttributeMetadata attmet = AttributeDao.GetMetadata(orgSvc, d.DependentComponentObjectId.Value);
                    string attributeLabel = attmet.DisplayName.UserLocalizedLabel.Label;

                    Console.WriteLine(
                        "An {0} named {1} will prevent deleting the {2} global option set.",
                        d.DependentComponentType.Value,
                        attributeLabel,
                        globalOptionSetName);
                }
            }
        }

        private string PromptOptionSetName()
        {
            Console.WriteLine();

            while (true)
            {
                Console.WriteLine("Enter option set name:");
                string input = Console.ReadLine();
                if (input != null && input != string.Empty)
                {
                    return input;
                }
                Console.WriteLine("Name cannot be empty. Try again.");
            }
        }
    }
}
