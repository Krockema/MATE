using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Models.DB;

namespace Master40.BusinessLogic.Helper
{
    public interface IWorkSchedule
    {
        int HierarchyNumber { get; set; }
        string Name { get; set; }
        int Duration { get; set; }
        int MachineGroupId { get; set; }
    }
}
