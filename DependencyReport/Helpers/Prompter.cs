using System;

namespace CoreySutton.Xrm.Tooling.DependencyReport
{
    public static class Prompter
    {
        public static FunctionType ChooseFunction()
        {
            Console.WriteLine();
            Console.WriteLine("1) Show solution dependencies");
            Console.WriteLine("1) Mark attributes as depricated");
            Console.WriteLine("3) Show attributes with no dependencies");
            Console.WriteLine("4) Show global option set dependencies");

            string range = "(1-4)";
            int lowerRange = 1;
            int upperRange = 4;

            while (true)
            {
                Console.Write($"Select report type {range}[1]: ");
                string input = Console.ReadLine();
                if (input == null || input == string.Empty)
                {
                    return FunctionType.ShowSolutionDependencies;
                }

                if (int.TryParse(input, out int reportType))
                {
                    if (lowerRange <= reportType && reportType <= upperRange)
                    {
                        return (FunctionType)reportType;
                    }

                    Console.WriteLine($"Selection out of range {range}. Try again.");
                }
                else
                {
                    Console.WriteLine("Selection must be a number. Try again.");
                }
            }
        }

        public static string SolutionName()
        {
            while (true)
            {
                Console.Write("Enter solution name: ");
                string input = Console.ReadLine();
                if (input != null && input != string.Empty)
                {
                    return input;
                }
                Console.WriteLine("Name cannot be empty. Try again.");
            }
        }

        public static bool YesNo(string query, bool defaultOption)
        {
            while (true)
            {
                Console.Write($"{query} (Y-N)[{(defaultOption ? "Y" : "N")}]: ");
                string input = Console.ReadLine();
                if (input == null || input == string.Empty)
                {
                    return defaultOption;
                }
                else if (input.Length != 1)
                {
                    Console.WriteLine("Expected 1 character. Try again.");
                }
                else if (input.ToLower() == "y")
                {
                    return true;
                }
                else if (input.ToLower() == "n")
                {
                    return false;
                }
                else
                {
                    Console.WriteLine("Expected \"Y\" or \"N\". Try again.");
                }
            }
        }

    }
}
