using System;
using System.Collections.Generic;
using CoreySutton.Xrm.Utilities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CoreySutton.Xrm.Tooling.Core
{
    public static class SolutionUtil
    {
        public static Entity GetSolution(IOrganizationService organizationService)
        {
            // Get solutions
            QueryExpression query = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("ismanaged", ConditionOperator.Equal, false)
                    }
                }
            };
            List<Entity> solutions = organizationService.RetrieveMultiple<Entity>(query);
            if (solutions == null)
            {
                Console.WriteLine("No solutions found");
                return null;
            }

            // Display solutions
            Console.WriteLine("Select a solution:");
            int count = 0;
            foreach (Entity solution in solutions)
            {
                Console.WriteLine($"({count++})" +
                                  $" {solution.GetAttributeValue<string>("friendlyname")}" +
                                  $" {solution.GetAttributeValue<string>("uniquename")}");
            }

            // Prompt to select a solution
            int selection = -1;
            while (selection < 0)
            {
                Console.Write(">> ");
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input, out selection))
                {
                    Console.WriteLine("Invalid selection");
                }
            }

            // Get solution
            Entity selectedSolution = solutions[selection];
            Console.WriteLine($"Friendly Name: {selectedSolution.GetAttributeValue<string>("friendlyname")}");
            Console.WriteLine($"Unique Name: {selectedSolution.GetAttributeValue<string>("uniquename")}");
            Console.WriteLine($"Version Number: {selectedSolution.GetAttributeValue<string>("version")}");

            return selectedSolution;
        }
    }
}
