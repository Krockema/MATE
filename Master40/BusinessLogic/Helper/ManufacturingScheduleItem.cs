using System.Collections.Generic;

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
