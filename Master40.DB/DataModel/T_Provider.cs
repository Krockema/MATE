using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;

namespace Master40.DB.DataModel
{
    public class T_Provider : BaseEntity
    {

        public int ProviderId { get; set; }
        
        public override string ToString()
        {
            return Id.ToString();
        }

        public Id GetProviderId()
        {
            return new Id(ProviderId);
        }
    }
}