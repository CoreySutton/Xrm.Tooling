using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace CoreySutton.Xrm.Tooling.DependencyReport
{
    public class SolutionDao
    {
        public static Guid? GetId(IOrganizationService orgSvc, string uniqueName)
        {
            QueryByAttribute componentQuery = new QueryByAttribute
            {
                EntityName = Solution.EntityLogicalName,
                ColumnSet = new ColumnSet(nameof(Solution.SolutionId).ToLower()),
                Attributes = { nameof(Solution.UniqueName).ToLower() },
                Values = { uniqueName }
            };

            return orgSvc.RetrieveMultiple(componentQuery).Entities.FirstOrDefault()?.Id;
        }

        public static string GetName(IOrganizationService orgSvc, Guid solutionId)
        {
            Solution solution = orgSvc.Retrieve(
                Solution.EntityLogicalName,
                solutionId,
                new ColumnSet(nameof(Solution.FriendlyName).ToLower()))
                .ToEntity<Solution>();

            return solution.FriendlyName;
        }
    }
}