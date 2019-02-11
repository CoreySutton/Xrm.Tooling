using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;

namespace CoreySutton.Xrm.Tooling.DependencyReport
{
    public class ComponentInfoProvider
    {
        private readonly IOrganizationService _orgSvc;

        public ComponentInfoProvider(IOrganizationService orgSvc)
        {
            _orgSvc = orgSvc;
        }

        public ComponentInfo GetComponentInfo(componenttype componentType, Guid componentId)
        {
            switch (componentType)
            {
                case componenttype.OptionSet: return GetGlobalOptionSetName(componentId);
                case componenttype.Attribute: return GetAttributeInformation(componentId);
                case componenttype.Entity: return GetEntityName(componentId);
                case componenttype.Relationship: return GetRelationshipName(componentId);
                case componenttype.EntityRelationship: return GetRelationshipName(componentId);
                // These need to be implemented
                case componenttype.AttributePicklistValue:
                case componenttype.AttributeLookupValue:
                case componenttype.ViewAttribute:
                case componenttype.LocalizedLabel:
                case componenttype.RelationshipExtraCondition:
                case componenttype.EntityRelationshipRole:
                case componenttype.EntityRelationshipRelationships:
                case componenttype.ManagedProperty:
                case componenttype.EntityKey:
                case componenttype.Privilege:
                case componenttype.PrivilegeObjectTypeCode:
                case componenttype.Role:
                case componenttype.RolePrivilege:
                case componenttype.DisplayString:
                case componenttype.DisplayStringMap:
                case componenttype.Form:
                case componenttype.Organization:
                case componenttype.SavedQuery:
                case componenttype.Workflow:
                case componenttype.Report:
                case componenttype.ReportEntity:
                case componenttype.ReportCategory:
                case componenttype.ReportVisibility:
                case componenttype.Attachment:
                case componenttype.EmailTemplate:
                case componenttype.ContractTemplate:
                case componenttype.KBArticleTemplate:
                case componenttype.MailMergeTemplate:
                case componenttype.DuplicateRule:
                case componenttype.DuplicateRuleCondition:
                case componenttype.EntityMap:
                case componenttype.AttributeMap:
                case componenttype.RibbonCommand:
                case componenttype.RibbonContextGroup:
                case componenttype.RibbonCustomization:
                case componenttype.RibbonRule:
                case componenttype.RibbonTabToCommandMap:
                case componenttype.RibbonDiff:
                case componenttype.SavedQueryVisualization:
                case componenttype.SystemForm:
                case componenttype.WebResource:
                case componenttype.SiteMap:
                case componenttype.ConnectionRole:
                case componenttype.ComplexControl:
                case componenttype.FieldSecurityProfile:
                case componenttype.FieldPermission:
                case componenttype.PluginType:
                case componenttype.PluginAssembly:
                case componenttype.SDKMessageProcessingStep:
                case componenttype.SDKMessageProcessingStepImage:
                case componenttype.ServiceEndpoint:
                case componenttype.RoutingRule:
                case componenttype.RoutingRuleItem:
                case componenttype.SLA:
                case componenttype.SLAItem:
                case componenttype.ConvertRule:
                case componenttype.ConvertRuleItem:
                case componenttype.HierarchyRule:
                case componenttype.MobileOfflineProfile:
                case componenttype.MobileOfflineProfileItem:
                case componenttype.SimilarityRule:
                case componenttype.CustomControl:
                case componenttype.CustomControlDefaultConfig:
                case componenttype.DataSourceMapping:
                case componenttype.SDKMessage:
                case componenttype.SDKMessageFilter:
                case componenttype.SdkMessagePair:
                case componenttype.SdkMessageRequest:
                case componenttype.SdkMessageRequestField:
                case componenttype.SdkMessageResponse:
                case componenttype.SdkMessageResponseField:
                case componenttype.WebWizard:
                case componenttype.Index:
                case componenttype.ImportMap:
                case componenttype.CanvasApp:
                default: return new ComponentInfo
                {
                    Name = "<N/A>"
                };
            }
        }

        public ComponentInfo GetAttributeInformation(Guid id)
        {
            var req = new RetrieveAttributeRequest
            {
                MetadataId = id
            };

            var resp = (RetrieveAttributeResponse)_orgSvc.Execute(req);

            AttributeMetadata attmet = resp.AttributeMetadata;

            return new ComponentInfo
            {
                EntityLogicalName = attmet.EntityLogicalName,
                Name = attmet.DisplayName.UserLocalizedLabel.Label,
                IsManaged = attmet.IsManaged,
                LogicalName = attmet.LogicalName,
                ComponentType = componenttype.Attribute,
                ComponentId = id,
                RequiredLevel = attmet.RequiredLevel.Value,
                IsValidForAdvancedFind = attmet.IsValidForAdvancedFind.Value
            };
        }

        public ComponentInfo GetGlobalOptionSetName(Guid id)
        {
            var req = new RetrieveOptionSetRequest
            {
                MetadataId = id
            };

            var resp = (RetrieveOptionSetResponse)_orgSvc.Execute(req);

            return new ComponentInfo
            {
                Name = resp.OptionSetMetadata.DisplayName.UserLocalizedLabel.Label,
                IsManaged = resp.OptionSetMetadata.IsManaged,
                ComponentType = componenttype.OptionSet,
                ComponentId = id
            };
        }

        public ComponentInfo GetEntityName(Guid id)
        {
            var req = new RetrieveEntityRequest
            {
                MetadataId = id
            };

            var resp = (RetrieveEntityResponse)_orgSvc.Execute(req);

            return new ComponentInfo
            {
                Name = resp.EntityMetadata.DisplayName.UserLocalizedLabel.Label,
                IsManaged = resp.EntityMetadata.IsManaged,
                ComponentType = componenttype.Entity,
                ComponentId = id
            };
        }

        public ComponentInfo GetRelationshipName(Guid id)
        {
            var req = new RetrieveRelationshipRequest
            {
                MetadataId = id
            };

            var resp = (RetrieveRelationshipResponse)_orgSvc.Execute(req);

            return new ComponentInfo
            {
                Name = resp.RelationshipMetadata.SchemaName,
                IsManaged = resp.RelationshipMetadata.IsManaged,
                ComponentType = componenttype.Relationship,
                ComponentId = id
            };
        }
    }
}
