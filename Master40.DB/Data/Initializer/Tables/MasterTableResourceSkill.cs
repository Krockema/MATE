using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableResourceSkill
    {
        internal M_ResourceSkill CUTTING;
        internal M_ResourceSkill DRILLING;
        internal M_ResourceSkill ASSEMBLING;

        internal MasterTableResourceSkill()
        {
            CUTTING = new M_ResourceSkill { Name = "Cutting" };
            DRILLING = new M_ResourceSkill { Name = "Drilling" };
            ASSEMBLING = new M_ResourceSkill { Name = "Assembling" };
        }

        internal M_ResourceSkill[] Init(MasterDBContext context)
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