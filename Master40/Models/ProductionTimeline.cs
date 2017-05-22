using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models
{

    /* sample
            {
                "name": " Step A ", "desc": "&rarr; Step B", "values": 
                [{ "id": "b0", "from": "/Date(1320182000000)/",
                    "to": "/Date(1320301600000)/",
                    "desc": "Id: 0<br/>Name:   Step A",
                    "label": " Step A", "customClass": "ganttRed", "dep": "b1" },
                 { "id": "bx", "from": "/Date(1320601600000)/", "to": "/Date(1320870400000)/", "desc": "Id: 0<br/>Name:   Step A", "label": " Step A", "customClass": "ganttRed", "dep": "b1" }]
            },
    */
    [JsonObject]
    public class ProductionTimeline
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("desc")]
        public string Desc { get; set; }
        [JsonProperty("values")]
        public List<ProductionTimelineItem> Values { get; set; }
        
    }

    [JsonObject]
    public class ProductionTimelineItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("from")]
        public string From { get; set; }
        [JsonProperty("to")]
        public string To { get; set; }
        [JsonProperty("desc")]
        public string Desc { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("customClass")]
        public string CustomClass { get; set; }
        [JsonProperty("dep")]
        public string Dep { get; set; }
        
    }
}
