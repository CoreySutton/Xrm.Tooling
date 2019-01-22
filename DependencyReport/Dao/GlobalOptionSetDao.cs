using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreySutton.Xrm.Tooling.DependencyReport
{
    public static class GlobalOptionSetDao
    {
        public static OptionSetMetadata GetMetadata(IOrganizationService orgSvc, string globalOptionSetName)
        {
            var retrieveOptionSetRequest = new RetrieveOptionSetRequest
            {
                Name = globalOptionSetName
            };

            var retrieveOptionSetResponse = (RetrieveOptionSetResponse)orgSvc.Execute(retrieveOptionSetRequest);
            return (OptionSetMetadata)retrieveOptionSetResponse.OptionSetMetadata;
        }

        public static Guid? GetId(IOrganizationService orgSvc, string globalOptionSetName)
        {
            return GetMetadata(orgSvc, globalOptionSetName).MetadataId;
        }

        public static IEnumerable<Dependency> GetDependencies(IOrganizationService orgSvc, Guid globalOptionSetId)
        {
            var retrieveDependenciesForDeleteRequest = new RetrieveDependenciesForDeleteRequest
            {
                ComponentType = (int)componenttype.OptionSet,
                ObjectId = globalOptionSetId
            };

            var retrieveDependenciesForDeleteResponse =
                (RetrieveDependenciesForDeleteResponse)orgSvc.Execute(retrieveDependenciesForDeleteRequest);

            return retrieveDependenciesForDeleteResponse.EntityCollection.Entities.Select(e => e.ToEntity<Dependency>());
        }
    }
}
