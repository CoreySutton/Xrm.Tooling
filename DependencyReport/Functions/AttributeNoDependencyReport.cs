using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoreySutton.Xrm.Tooling.DependencyReport
{
    /// <summary>
    /// Reports on attributes with no dependencies
    /// </summary>
    public class AttributeNoDependencyReport
    {
        private readonly IOrganizationService _orgSvc;
        private readonly ComponentInfoProvider cInfoProvider;
        private readonly StringBuilder _sb;
        private bool _onlyUnmanaged;
        private bool _skipBase;
        private bool _runDepricator;

        public AttributeNoDependencyReport(IOrganizationService orgSvc)
        {
            _orgSvc = orgSvc;
            cInfoProvider = new ComponentInfoProvider(orgSvc);
            _sb = new StringBuilder();
            _sb.AppendLine("Name,Id,Type,Entity Logical Name,Managed,Has Data");
        }

        public IList<ComponentInfo> Find()
        {
            Guid solutionId = GetSolutionId();
            
            _onlyUnmanaged = Prompter.YesNo("Only get unmanaged", true);
            _skipBase = Prompter.YesNo("Skip \"(base)\" attributes", true);
            _runDepricator = Prompter.YesNo("Depricate attributes with no dependencies", true);

            IEnumerable<SolutionComponent> attributeComponents = SolutionComponentDao.GetSolutionComponents(
                _orgSvc, 
                solutionId, 
                componenttype.Attribute);

            IList<ComponentInfo> cInfos = new List<ComponentInfo>();
            foreach (SolutionComponent component in attributeComponents)
            {
                ComponentInfo cInfo = ProcessSolutionComponent(component.ObjectId.Value);
                if (cInfo != null) cInfos.Add(cInfo);
            }

            SaveFile();

            return cInfos;
        }

        private Guid GetSolutionId()
        {
            while (true) {
                string solutionName = Prompter.SolutionName();
                Guid? solutionId = SolutionDao.GetId(_orgSvc, solutionName);
                if (solutionId != null) return solutionId.Value;
            }
        }

        private ComponentInfo ProcessSolutionComponent(Guid componentObjectId)
        {
            ComponentInfo cInfo = cInfoProvider.GetAttributeInformation(componentObjectId);

            if (ShouldSkip(cInfo)) return null;            
            if (SolutionComponentDao.HasDependencies(_orgSvc, cInfo.ComponentType, cInfo.ComponentId)) return null;

            bool hasData = HasData(cInfo);
            PrintComponentInfo(cInfo, hasData);
            AppendToFile(cInfo, hasData);

            return cInfo;
        }

        private bool ShouldSkip(ComponentInfo cInfo)
        {
            // Skip as dont want managed attributes
            if (_onlyUnmanaged && cInfo.IsManaged != false) return true;

            // Skip as don't want base attributes
            if (_skipBase && cInfo.Name.Contains("(Base)")) return true;

            // Skip as don't want deprecated attributes
            if (_runDepricator && cInfo.Name.Contains("[DEP]")) return true;

            return false;
        }

        private bool HasData(ComponentInfo cInfo)
        {
            return EntityDao.CountRecordsContainingValue(_orgSvc, cInfo.EntityLogicalName, cInfo.LogicalName);
        }

        private void PrintComponentInfo(ComponentInfo cInfo, bool hasData)
        {
            // name
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{cInfo.Name}");

            // entity name
            if (cInfo.EntityLogicalName != null) Console.Write($" - {cInfo.EntityLogicalName}");
            Console.WriteLine();

            // guid and type
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\t{cInfo.ComponentId} | {cInfo.ComponentType}");
                        
            // has data
            if (hasData) Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\tHas Data: {hasData}");

            // is managed
            if (cInfo.IsManaged != null)
            {
                if (cInfo.IsManaged == true)                    
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                Console.WriteLine($"\tManaged: {cInfo.IsManaged.Value}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            // reset color
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void AppendToFile(ComponentInfo cInfo, bool hasData)
        {
            _sb.AppendLine(
                $"{cInfo.Name},{cInfo.ComponentId},{cInfo.ComponentType}," +
                $"{cInfo.EntityLogicalName},{cInfo.IsManaged},{hasData}");
        }

        private void SaveFile()
        {
            bool tryAgain = true;
            while (tryAgain)
            {
                try
                {
                    File.WriteAllText(@"report.csv", _sb.ToString());
                    tryAgain = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write to file: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);

                    tryAgain = Prompter.YesNo("Try again", true);
                }
            }
        }
    }
}
