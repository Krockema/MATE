namespace Master40.DB.ReportingModel.Interface
{

    public interface ISimulationTask
    {
        long Start { get; set; }
        long End { get; set; }
        string Mapping { get; }
    }
}

