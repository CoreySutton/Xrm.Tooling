using System;
using System.Collections.Generic;
using System.Linq;
using CoreySutton.Utilities;
using CoreySutton.Xrm.Utilities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;


namespace CoreySutton.Xrm.Tooling.Core
{
    public static class SolutionUtil
    {
        public static Entity GetSolution(IOrganizationService organizationService, bool allowNew = false)
        {
            // Get all unmanaged solutions
            List<Entity> solutions = GetUnmanagedSolutions(organizationService);
            if (solutions == null)
            {
                Console.WriteLine("No solutions found");
                return null;
            }

            // Pick solution
            Entity selectedSolution = PromptPickSolution(solutions, allowNew);
            if (selectedSolution == null) return null;

            // Display solution information
            Console.WriteLine($"Friendly Name: {selectedSolution.GetAttributeValue<string>("friendlyname")}");
            Console.WriteLine($"Unique Name: {selectedSolution.GetAttributeValue<string>("uniquename")}");
            Console.WriteLine($"Version Number: {selectedSolution.GetAttributeValue<string>("version")}");

            return selectedSolution;
        }

        public static List<Entity> GetUnmanagedSolutions(IOrganizationService organizationService)
        {
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

            return organizationService.RetrieveMultiple<Entity>(query);
        }

        public static Entity GetSolutionByName(
            IOrganizationService organizationService,
            string uniqueName)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("uniquename", ConditionOperator.Equal, uniqueName)
                    }
                }
            };

            IList<Entity> solutions = organizationService.RetrieveMultiple<Entity>(query);
            if (Validator.IsNullOrEmpty(solutions))
            {
                return null;
            }

            if (solutions.Count > 1)
            {
                throw new Exception($"Found {solutions.Count} solutions with unique name {uniqueName}, expected 1");
            }

            return solutions.First();
        }

        public static IList<Entity> GetSolutionsByName(
            IOrganizationService organizationService,
            string[] uniqueNames)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("uniquename", ConditionOperator.In, uniqueNames)
                    }
                }
            };

            IList<Entity> solutions = organizationService.RetrieveMultiple<Entity>(query);
            return Validator.IsNullOrEmpty(solutions) ? null : solutions;
        }

        public static void PrintSolutions(IList<Entity> solutions, bool allowNew = false)
        {
            Argument.IsNotNullOrEmpty(solutions, nameof(solutions));

            // Display solutions
            int count = 1;

            if (allowNew)
            {
                Console.WriteLine("(0) **Create New Solution**");
            }

            foreach (Entity solution in solutions)
            {
                Console.WriteLine($"({count++})" +
                                  $" {solution.GetAttributeValue<string>("friendlyname")}" +
                                  $" {solution.GetAttributeValue<string>("uniquename")}");
            }
        }

        public static Entity PromptPickSolution(IList<Entity> solutions, bool allowNew = false)
        {
            Argument.IsNotNullOrEmpty(solutions, nameof(solutions));

            // Prompt to select a solution
            int selection = -1;
            Console.Write(">> Select a solution:");
            while (selection < 0)
            {
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) || !int.TryParse(input, out selection))
                {
                    Console.WriteLine("Invalid selection");
                }
            }

            return selection == 0 ? null : solutions[selection - 1];
        }
    }
}
