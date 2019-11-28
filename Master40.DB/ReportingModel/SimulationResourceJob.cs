﻿using System.ComponentModel.DataAnnotations.Schema;
using Master40.DB.Enums;
using Master40.DB.ReportingModel.Interface;

namespace Master40.DB.ReportingModel
{
    public class SimulationResourceJob : BaseEntity, ISimulationResourceData
    {
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public long Time { get; set; }
        public string JobName { get; set; }
        public string JobType { get; set; }
        public string Article { get; set; }
        public long ExpectedDuration { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public string Resource { get; set; }
        public string ResourceTool { get; set; }
        public string JobId { get; set; }
        public string ProductionOrderId { get; set; }
        public int DueTime { get; set; }
        public string OrderId { get; set; }
        public string CreatedForOrderId { get; set; }
        public int HierarchyNumber { get; set; }
        public string Parent { get; set; }
        public string ParentId { get; set; }
        public string Bucket { get; set; }
        
        [NotMapped]
        public string ArticleType { get; set; }


    }
}