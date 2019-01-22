using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace CoreySutton.Xrm.Tooling.DependencyReport
{
    public class AttributeDeprecator
    {
        public bool Prompt { get; set; }

        private readonly IOrganizationService _orgSvc;

        public AttributeDeprecator(IOrganizationService orgSvc, bool prompt = true)
        {
            _orgSvc = orgSvc;
            Prompt = true;
        }

        public void Process(IList<ComponentInfo> cInfos)
        {
            foreach (ComponentInfo cInfo in cInfos)
            {
                if (!cInfo.Name.Contains("[DEP]"))
                {
                    if (Prompt == false || Prompter.YesNo("Depricate Attribute", true))
                    {
                        AttributeDao.Deprecate(_orgSvc, cInfo.ComponentId, cInfo.EntityLogicalName, cInfo.Name);
                        Console.WriteLine($"{cInfo.Name} - Attribute Depricated");
                    }
                }
            }
        }
    }
}
