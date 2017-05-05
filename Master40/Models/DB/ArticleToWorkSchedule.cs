using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ArticleToWorkSchedule
    {
        public int ArticleId { get; set; }
        public int WorkScheduleId { get; set; }

        public WorkSchedule WorkSchedule { get; set; }
        public Article Article { get; set; }
        public int Duration { get; set; }
    }
}
