using Mate.DataCore.Data.Context;
using Mate.DataCore.DataModel;

namespace Mate.DataCore.Data.Initializer.Tables
{
    internal class MasterTableValueType
    {
        internal M_ValueType mm;
        internal M_ValueType coordinate;
        internal M_ValueType degree;

        internal MasterTableValueType()
        {
            mm = new M_ValueType { Type = "mm" };
            coordinate = new M_ValueType { Type = "coordinate" };
            degree = new M_ValueType { Type = "degree" };
        }

        internal M_ValueType[] Init(MateDb context)
        {
            var valueTypes = new M_ValueType[]
            {
                mm,
                coordinate,
                degree
            };
            context.ValueTypes.AddRange(entities: valueTypes);
            context.SaveChanges();
            return valueTypes;
        }
    }
}
