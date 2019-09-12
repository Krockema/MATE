using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal static class MasterTableResourceTool
    {
        internal static M_ResourceTool SAW_BLADE_BIG = new M_ResourceTool { Name = "Saw blade big" };
        internal static M_ResourceTool SAW_BLADE_SMALL = new M_ResourceTool { Name = "Saw blade small" };
        internal static M_ResourceTool DRILL_HEAD_M6 = new M_ResourceTool { Name = "Drill head M6" };
        internal static M_ResourceTool DRILL_HEAD_M4 = new M_ResourceTool { Name = "Drill head M4" };
        internal static M_ResourceTool ASSEMBLY_SCREWDRIVER = new M_ResourceTool { Name = "Screwdriver universal" };
        
        internal static M_ResourceTool[] Init(MasterDBContext context)
        {
            var resourceTools = new M_ResourceTool[]
            {
                SAW_BLADE_BIG,
                SAW_BLADE_SMALL,
                DRILL_HEAD_M6,
                DRILL_HEAD_M4,
                ASSEMBLY_SCREWDRIVER
            };
            context.ResourceTools.AddRange(entities: resourceTools);
            context.SaveChanges();
            return resourceTools;
        }
    }
}