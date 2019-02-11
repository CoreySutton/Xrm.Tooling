using CoreySutton.Utilities;
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

        public static void HideFromSearch(IOrganizationService orgSvc, Guid attributeId, string entityName, string name)
        {
            var retrieveAttributeRequest = new UpdateAttributeRequest
            {
                EntityName = entityName,
                Attribute = new AttributeMetadata
                {
                    MetadataId = attributeId,
                    IsValidForAdvancedFind = new BooleanManagedProperty(false)
                }
            };
            var retrieveAttributeResponse = (UpdateAttributeResponse)orgSvc.Execute(retrieveAttributeRequest);
        }

        public static void Deprecate(IOrganizationService orgSvc, ComponentInfo cInfo)
        {
            Argument.IsNotNull(orgSvc, nameof(orgSvc));
            Argument.IsNotNull(cInfo, nameof(cInfo));
            Argument.IsNotNullOrEmpty(cInfo.EntityLogicalName, nameof(cInfo.EntityLogicalName));

            UpdateAttributeRequest request = CreateRequestStub(cInfo.ComponentId, cInfo.EntityLogicalName);

            if (!cInfo.Name.Contains("[DEP]"))
            {
                var newName = $"[DEP] {cInfo.Name}";
                if (newName.Length > 50) newName = newName.Substring(0, 50);
                request = SetName(request, newName);
            }

            if (cInfo.IsValidForAdvancedFind == true)
            {
                request = SetIsValidForAdvancedFind(request, false);
            }

            if (request.Attribute.IsValidForAdvancedFind != null || request.Attribute.DisplayName != null)
            {
                orgSvc.Execute(request);
            }
        }

        private static UpdateAttributeRequest CreateRequestStub(Guid attributeId, string entityName)
        {
            return new UpdateAttributeRequest
            {
                EntityName = entityName,
                Attribute = new AttributeMetadata
                {
                    MetadataId = attributeId
                }
            };
        }

        private static UpdateAttributeRequest SetName(UpdateAttributeRequest request, string name)
        {
            Argument.IsNotNull(request, nameof(request));
            Argument.IsNotNull(request.Attribute, nameof(request.Attribute));

            request.Attribute.DisplayName = new Label(name, 1033);
            return request;
        }

        private static UpdateAttributeRequest SetIsValidForAdvancedFind(UpdateAttributeRequest request, bool isValidForAdvancedFind)
        {
            Argument.IsNotNull(request, nameof(request));
            Argument.IsNotNull(request.Attribute, nameof(request.Attribute));

            request.Attribute.IsValidForAdvancedFind = new BooleanManagedProperty(isValidForAdvancedFind);
            return request;
        }
    }
}
