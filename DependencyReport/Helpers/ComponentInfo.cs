using System;
using Microsoft.Xrm.Sdk.Metadata;

namespace CoreySutton.Xrm.Tooling.DependencyReport
{
    public class ComponentInfo {
        public string Name { get; set; }
        public string LogicalName { get; set; }
        public bool? IsManaged { get; set; }
        public string EntityLogicalName { get; set; }
        public componenttype ComponentType { get; set; }
        public Guid ComponentId { get; set; }
        public AttributeRequiredLevel RequiredLevel { get; set; }
        public bool IsValidForAdvancedFind { get; set; }
    }
}
