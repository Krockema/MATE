using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.DataModel
{
    public class T_Demand : BaseEntity
    {

        public int DemandId { get; set; }
        
        public override string ToString()
        {
            return Id.ToString();
        }

        public Id GetDemandId()
        {
            return new Id(DemandId);
        }
    }
}