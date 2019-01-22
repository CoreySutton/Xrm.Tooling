using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreySutton.Xrm.Tooling.DependencyReport
{
    public class SolutionComponentDependencyReport
    {
        private readonly IOrganizationService _orgSvc;
        private readonly ComponentInfoProvider _componentTypeDao;
        private readonly OptionSetMetadata _componentTypeOptionSet;

        public SolutionComponentDependencyReport(IOrganizationService orgSvc)
        {
            _orgSvc = orgSvc;
            _componentTypeDao = new ComponentInfoProvider(orgSvc);
            //The ComponentType global Option Set contains options for each possible component.
            _componentTypeOptionSet = GlobalOptionSetDao.GetMetadata(_orgSvc, "componenttype");
        }

        public void Create()
        {
            Guid solutionId = GetSolutionId();
            bool detailedReport = Prompter.YesNo("Detailed report", true);

            IEnumerable<SolutionComponent> allComponents = SolutionComponentDao.GetSolutionComponents(_orgSvc, solutionId);

            foreach (SolutionComponent component in allComponents)
            {
                IEnumerable<Dependency> dependencies = SolutionComponentDao.GetDependencies(
                    _orgSvc,
                    component.ComponentType.Value,
                    component.ObjectId.Value);

                PrintDependencySummary(dependencies.Count(), component);

                //A more complete report requires more code
                if (dependencies.Any() && detailedReport)
                {                    
                    foreach (Dependency d in dependencies)
                    {
                        PrintDependencyDetails(d, component);
                    }
                }
            }
        }

        private Guid GetSolutionId()
        {
            while (true) {
                string solutionName = Prompter.SolutionName();
                Guid? solutionId = SolutionDao.GetId(_orgSvc, solutionName);
                if (solutionId != null) return solutionId.Value;
            }
        }
        
        private void PrintDependencySummary(int dependenciesCount, SolutionComponent component)
        {
            ComponentInfo componentInfo = _componentTypeDao.GetComponentInfo(component.ComponentType.Value, component.ObjectId.Value);

            if (dependenciesCount == 0)
                Console.ForegroundColor = ConsoleColor.Green;
            else
                Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(
                    $"{dependenciesCount} dependencies - {componentInfo.Name} ({component.ObjectId.Value})" +
                    $" ({component.ComponentType.Value})");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void PrintDependencyDetails(Dependency dependency, SolutionComponent component)
        {
            //These strings represent parameters for the message.
            ComponentInfo dependentComponentInfo;
            string dependentComponentTypeName = "";
            string dependentComponentSolutionName = "";
            ComponentInfo requiredComponentInfo;
            string requiredComponentTypeName = "";
            string requiredComponentSolutionName = "";

            // Match the Component type with the option value and get the label value of the option.
            foreach (OptionMetadata opt in _componentTypeOptionSet.Options)
            {
                if ((int)dependency.DependentComponentType.Value == opt.Value)
                {
                    dependentComponentTypeName = opt.Label.UserLocalizedLabel.Label;
                }

                if ((int)dependency.RequiredComponentType.Value == opt.Value)
                {
                    requiredComponentTypeName = opt.Label.UserLocalizedLabel.Label;
                }
            }

            //The name or display name of the component is retrieved in different ways depending on the component type
            dependentComponentInfo = _componentTypeDao.GetComponentInfo(
                dependency.DependentComponentType.Value,
                dependency.DependentComponentObjectId.Value);

            requiredComponentInfo = _componentTypeDao.GetComponentInfo(
                dependency.RequiredComponentType.Value,
                dependency.RequiredComponentObjectId.Value);

            // Retrieve the friendly name for the dependent solution.
            dependentComponentSolutionName = SolutionDao.GetName(_orgSvc, dependency.DependentComponentBaseSolutionId.Value);

            // Retrieve the friendly name for the required solution.
            requiredComponentSolutionName = SolutionDao.GetName(_orgSvc, dependency.RequiredComponentBaseSolutionId.Value);

            //Display the message
            Console.WriteLine(
                "The {0} {1} in the {2} depends on the {3} {4} in the {5} solution.",
                dependentComponentInfo.Name,
                dependentComponentTypeName,
                dependentComponentSolutionName,
                requiredComponentInfo.Name,
                requiredComponentTypeName,
                requiredComponentSolutionName);
        }
    }
}
