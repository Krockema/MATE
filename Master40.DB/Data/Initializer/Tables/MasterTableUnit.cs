using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;

namespace Master40.DB.Data.Initializer.Tables
{
    public class MasterTableUnit
    {
        public static M_Unit KILO = new M_Unit { Name = "Kilo" };
        public static M_Unit LITER = new M_Unit { Name = "Liter" };
        public static M_Unit PIECES = new M_Unit { Name = "Pieces" };
        public static M_Unit[] Init(MasterDBContext context)
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
