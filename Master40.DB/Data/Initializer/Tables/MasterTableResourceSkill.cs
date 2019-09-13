using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal static class MasterTableResourceSkill
    {
        internal static M_ResourceSkill CUTTING = new M_ResourceSkill { Name = "Cutting" };
        internal static M_ResourceSkill DRILLING = new M_ResourceSkill {Name = "Drilling" };
        internal static M_ResourceSkill ASSEMBLING = new M_ResourceSkill { Name = "Assembling" };
        internal static  M_ResourceSkill[] Init(MasterDBContext context)
        {
            var resourceSkills = new M_ResourceSkill[]
            {
                CUTTING,
                DRILLING,
                ASSEMBLING,
            };

            context.ResourceSkills.AddRange(resourceSkills);
            context.SaveChanges();
            return resourceSkills;
        }
    }
}