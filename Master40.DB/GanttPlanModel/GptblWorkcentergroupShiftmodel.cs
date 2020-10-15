using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkcentergroupShiftmodel
    {
        public string ClientId { get; set; }
        public string WorkcentergroupId { get; set; }
        public DateTime StartDate { get; set; }
        public string ShiftmodelId { get; set; }
        public int? StartShiftIndex { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}
