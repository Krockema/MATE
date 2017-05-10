using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Models.DB;

namespace Master40.BusinessLogic.Helper
{
    public class ManufacturingScheduleItem
    {
        
        public int MachineGroupId { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public int ProductionOrderId { get; set; }
        public int ArticleId { get; set; }
        public int Duration { get; set; }
        public List<int> ParentsArticleId { get; set; }
        public List<int> ChildrenArticleId { get; set; }
        

    }
}
