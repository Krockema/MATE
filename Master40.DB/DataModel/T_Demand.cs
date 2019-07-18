using System.Collections.Generic;
using System.Data;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.DataModel
{
    public class T_Demand : BaseEntity
    {
        // TODO: add unique constraint here
        public int DemandId { get; set; }
        
        public override string ToString()
        {
            return DemandId.ToString();
        }

        public Id GetDemandId()
        {
            return new Id(DemandId);
        }

        public T_Demand(Id demandId)
        {
            DemandId = demandId.GetValue();
        }
        
        public T_Demand(int demandId)
        {
            DemandId = demandId;
        }

        public T_Demand()
        {
        }
    }
}