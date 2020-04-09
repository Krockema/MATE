using Master40.DB.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.ReportingModel
{
    public class SimulationMeasurement : ResultBaseEntity
    {
        public Guid JobId { get; set; }
        public Guid ArticleKey { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationConfigurationId { get; set; }
        public int SimulationNumber { get; set; }
        public string JobName { get; set; }
        public string ArticleName { get; set; }
        public string CharacteristicName { get; set; }
        public string AttributeName { get; set; }
        public string Resource { get; set; }
        public string ResourceTool { get; set; }
        public double MeasurementValue { get; set; }
        public long TimeStamp { get; set; }
        public double TargetValue { get; set; }
    }
}
