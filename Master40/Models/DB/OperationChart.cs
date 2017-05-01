using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class OperationChart
    {
        public int OperationChartId { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public int? MachineToolId { get; set; }
        public MachineTool MachineTool { get; set; }
        public int? MachineId { get; set; }
        public Machine Machine { get; set; }
        public int ArticleBomPartId { get; set; }
    }
}