using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.Models
{
    public class ProductionSchedule
    {
        public class GanttContext 
        {
            public GanttContext()
            {
                Tasks = new List<GanttTask>();
                Links = new List<GanttLink>();
            }

            public List<GanttTask> Tasks { get; set; }
            public List<GanttLink> Links { get; set; }
        }

        [JsonObject]
        public class GanttTask
        {
            public int id { get; set; }
            public string text { get; set; }
            public string desc { get; set; }
            public DateTime start_date { get; set; }
            public int duration { get; set; }
            public DateTime end_date { get; set; }
            public double progress { get; set; }
            public int parent { get; set; }
            [JsonIgnore]
            public long IntFrom { get; set; }
            [JsonIgnore]
            public long IntTo { get; set; }
        }
    }

    public class GanttLink
    {
        public int id { get; set; }
        public int source { get; set; }
        public int target { get; set; }
        public string type { get; set; }
    }
}
