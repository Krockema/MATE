using System;
using Master40.PiWebApi.Interfaces;

namespace Master40.DB.ReportingModel
{
    public class SimulationMeasurement : ResultBaseEntity, IPiWebMeasurement
    {
        public Guid JobId { get; set; }
        public Guid ArticleKey { get; set; }
        public int SimulationType { get; set; }
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
