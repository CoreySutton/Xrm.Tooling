using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;

namespace CoreySutton.Xrm.Tooling.DependencyReport
{
    public static class AttributeDao
    {
        public static AttributeMetadata GetMetadata(IOrganizationService orgSvc, Guid attributeId)
        {
            var retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                MetadataId = attributeId
            };
            var retrieveAttributeResponse = (RetrieveAttributeResponse)orgSvc.Execute(retrieveAttributeRequest);

            return retrieveAttributeResponse.AttributeMetadata;
        }

        public static void Deprecate(IOrganizationService orgSvc, Guid attributeId, string entityName, string name)
        {
            name = $"[DEP] {name}";
            if (name.Length > 50) name = name.Substring(0, 50);

            UpdateName(orgSvc, attributeId, entityName, name);
        }

        public static void UpdateName(IOrganizationService orgSvc, Guid attributeId, string entityName, string name)
        {
            var retrieveAttributeRequest = new UpdateAttributeRequest
            {
                EntityName = entityName,
                Attribute = new AttributeMetadata
                {
                    MetadataId = attributeId,
                    DisplayName = new Label(name, 1033)
                }
            };
            var retrieveAttributeResponse = (UpdateAttributeResponse)orgSvc.Execute(retrieveAttributeRequest);
        }
    }
}
