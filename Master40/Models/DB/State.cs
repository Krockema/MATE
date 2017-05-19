using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public enum State
    {
        Created,
        ProviderExist,
        SchedulesExist,
        ExistsInCapacityPlan,
        Produced,
        Deliverd,
        Purchased,
    }



}
