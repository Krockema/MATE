using Mate.DataCore.Data.Context;
using Mate.DataCore.DataModel;

namespace Mate.DataCore.Data.Initializer.Tables
{
    public class MasterTableUnit
    {
        public M_Unit KILO;
        public M_Unit LITER;
        public M_Unit PIECES;

        public MasterTableUnit()
        {
            KILO = new M_Unit { Name = "Kilo" };
            LITER = new M_Unit { Name = "Liter" };
            PIECES = new M_Unit { Name = "Pieces" };
        }

        public M_Unit[] Init(MateDb context)
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
