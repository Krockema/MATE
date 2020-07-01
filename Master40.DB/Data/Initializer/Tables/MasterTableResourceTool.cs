using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableResourceTool
    {
        internal Dictionary<string , List<M_ResourceTool>> ResourceTools = new Dictionary<string, List<M_ResourceTool>>();

        internal MasterTableResourceTool(MasterTableResourceCapability capability, int sawTools, int drillTools, int assemblyTools)
        {
            CreateToolGroup(sawTools, capability.CUTTING.Name);
            CreateToolGroup(drillTools, capability.DRILLING.Name);
            CreateToolGroup(assemblyTools, capability.ASSEMBLING.Name);
        }

        private void CreateToolGroup(int numberTools, string name)
        {
            List<M_ResourceTool> tools = new List<M_ResourceTool>();
            for (int i = 0; i < numberTools; i++)
            {
                tools.Add(CreateNewTool(name, i));
            }
            ResourceTools.Add(name, tools);
        }

        private M_ResourceTool CreateNewTool(string toolName, int number)
        {
            return new M_ResourceTool() { Name = toolName + " " + number};
        }

        internal void Init(MasterDBContext context)
        {
            foreach (var item in ResourceTools)
            {
                context.ResourceTools.AddRange(entities: item.Value);
            }
            context.SaveChanges();
            
        }
    }
}