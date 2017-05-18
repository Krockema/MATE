using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogic.Helper;

namespace Master40.Models.DB
{
    public class WorkSchedule : IWorkSchedule
    {
        public int WorkScheduleId { get; set; }
        public int HierarchyNumber { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public int MachineGroupId { get; set; }
        public MachineGroup MachineGroup { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
    }
}