using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CoreySutton.Xrm.Tooling.DependencyReport
{
    public static class EntityDao
    {
        public static bool CountRecordsContainingValue(
            IOrganizationService orgSvc,
            string entityLogicalName, 
            string atttributeName)
        {
            var query = new QueryExpression(entityLogicalName)
            {
                ColumnSet = new ColumnSet(false),
                Criteria = {
                    Conditions =
                    {
                        new ConditionExpression(atttributeName, ConditionOperator.NotNull)
                    }
                }
            };

            EntityCollection ec = orgSvc.RetrieveMultiple(query);
            return ec.Entities.Count > 0;
        }
    }
}
