using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    public class MasterTableUnit
    {
        internal M_Unit KILO;
        internal M_Unit LITER;
        internal M_Unit PIECES;

        public MasterTableUnit()
        {
            KILO = new M_Unit { Name = "Kilo" };
            LITER = new M_Unit { Name = "Liter" };
            PIECES = new M_Unit { Name = "Pieces" };
        }

        public M_Unit[] Init(MasterDBContext context)
        {                                       
            // Units
            var units = new M_Unit[]
            {
                KILO,
                LITER,
                PIECES
            };
            context.Units.AddRange(entities: units);
            context.SaveChanges();
            return units;
        }
    }
}
