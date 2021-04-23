using System;

namespace Master40.PiWebApi.Interfaces
{
    public interface IPiWebMeasurement
    {
        Guid JobId { get; set; }
        Guid ArticleKey { get; set; }
        int SimulationType { get; set; }
        int SimulationConfigurationId { get; set; }
        int SimulationNumber { get; set; }
        string JobName { get; set; }
        string ArticleName { get; set; }
        string CharacteristicName { get; set; }
        string AttributeName { get; set; }
        string Resource { get; set; }
        string ResourceTool { get; set; }
        double MeasurementValue { get; set; }
        long TimeStamp { get; set; }
        double TargetValue { get; set; }
    }
}