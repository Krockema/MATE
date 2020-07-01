using System;
using System.Collections.Generic;
using Master40.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Master40.Models
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
        public string id { get; set; }
        public string text { get; set; }
        public string desc { get; set; }
        public GanttType type { get; set; }
        [JsonConverter(converterType: typeof(StringEnumConverter))]
        public GanttColors color { get; set; }
        public string start_date { get; set; }
        public int duration { get; set; }
        public string end_date { get; set; }
        public double progress { get; set; }
        public bool open => false;
        public string parent { get; set; }
        [JsonIgnore]
        public long IntFrom { get; set; }
        [JsonIgnore]
        public long IntTo { get; set; }
        [JsonIgnore]
        public int GroupId { get; set; }
    }

    public class GanttLink
    {
        public string id { get; set; }
        public string source { get; set; }
        public string target { get; set; }
        public LinkType type { get; set; }
    }

    public enum GanttType
    {
       task = 0,
       project = 1,
       milestone = 2
    }

    public enum LinkType
    {
        finish_to_start = 0,
        start_to_start = 1,
        finish_to_finish = 2,
        start_to_finish = 3 
    }

}
