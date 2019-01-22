using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreySutton.Xrm.Tooling.DependencyReport
{
    public class SolutionComponentDao
    {
        public static IEnumerable<SolutionComponent> GetSolutionComponents(
            IOrganizationService orgSvc,
            Guid solutionId,
            componenttype? componentType = null)
        {
            QueryByAttribute componentQuery = new QueryByAttribute
            {
                EntityName = SolutionComponent.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    nameof(SolutionComponent.ComponentType).ToLower(),
                    nameof(SolutionComponent.ObjectId).ToLower(),
                    nameof(SolutionComponent.SolutionComponentId).ToLower(),
                    nameof(SolutionComponent.SolutionId).ToLower()),
                Attributes = {
                    nameof(SolutionComponent.SolutionId).ToLower()
                },
                Values = {
                    solutionId
                }
            };

            if (componentType != null)
            {
                componentQuery.AddAttributeValue(nameof(SolutionComponent.ComponentType).ToLower(), (int)componentType);
            }

            return orgSvc.RetrieveMultiple(componentQuery).Entities.Select(e => e.ToEntity<SolutionComponent>());
        }

        public static IEnumerable<Dependency> GetDependencies(
            IOrganizationService orgSvc,
            componenttype componentType,
            Guid objectId)
        {
            var dependentComponentsRequest = new RetrieveDependentComponentsRequest
            {
                ComponentType = (int)componentType,
                ObjectId = objectId
            };

            var dependentComponentsResponse = (RetrieveDependentComponentsResponse)orgSvc.Execute(dependentComponentsRequest);
            return dependentComponentsResponse.EntityCollection.Entities.Select(e => e.ToEntity<Dependency>());
        }

        public static int CountDependencies(
            IOrganizationService orgSvc,
            componenttype componentType,
            Guid objectId)
        {
            return GetDependencies(orgSvc, componentType, objectId).Count();
        }

        public static bool HasDependencies(
            IOrganizationService orgSvc,
            componenttype componentType,
            Guid objectId)
        {
            return GetDependencies(orgSvc, componentType, objectId).Any();
        }
    }
}
