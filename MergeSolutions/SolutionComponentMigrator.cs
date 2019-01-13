//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.Crm.Sdk.Messages;
//using Microsoft.Xrm.Sdk;
//using Microsoft.Xrm.Sdk.Query;

//namespace CoreySutton.Xrm.Tooling.MergeSolutions
//{
//    public class SolutionComponentMigrator
//    {
//        private List<int> _componentTypes = new List<int>();

//        internal void CopyComponents(
//            IList<Entity> sourceSolutions,
//            IList<Entity> targetSolutions,
//            IOrganizationService organizationService)
//        {
//            Console.WriteLine("Retrieving source solution(s) components...");

//            List<Entity> components = RetrieveComponentsFromSolutions(
//                sourceSolutions.Select(s => s.Id).ToList(),
//                _componentTypes,
//                organizationService);

//            foreach (Entity target in targetSolutions)
//            {
//                Console.WriteLine($"Adding {components.Count} components to" +
//                                  $" solution '{target.GetAttributeValue<string>("friendlyname")}'");

//                foreach (Entity component in components)
//                {
//                    var request = new AddSolutionComponentRequest
//                    {
//                        AddRequiredComponents = false,
//                        ComponentId = component.GetAttributeValue<Guid>("objectid"),
//                        ComponentType = component.GetAttributeValue<OptionSetValue>("componenttype").Value,
//                        SolutionUniqueName = target.GetAttributeValue<string>("uniquename"),
//                    };

//                    // If CRM 2016 or above, handle subcomponents behavior
//                    if (settings.ConnectionDetail.OrganizationMajorVersion >= 8)
//                    {
//                        request.DoNotIncludeSubcomponents =
//                            component.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value == 1 ||
//                            component.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value == 2;
//                    }

//                    organizationService.Execute(request);
//                }
//            }
//        }

//        internal List<Entity> RetrieveComponentsFromSolutions(
//            IList<Guid> solutionsIds,
//            List<int> componentsTypes,
//            IOrganizationService organizationService)
//        {
//            var qe = new QueryExpression("solutioncomponent")
//            {
//                ColumnSet = new ColumnSet(true),
//                Criteria = new FilterExpression
//                {
//                    Conditions =
//                    {
//                        new ConditionExpression("solutionid", ConditionOperator.In, solutionsIds.ToArray()),
//                        new ConditionExpression("componenttype", ConditionOperator.In, componentsTypes.ToArray())
//                    }
//                }
//            };

//            return organizationService.RetrieveMultiple(qe).Entities.ToList();
//        }
//    }
//}
