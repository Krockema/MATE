namespace Master40.DataGenerator.DataModel.TransitionMatrix
{
    public class InputParameterSet
    {
        
        public int? WorkingStationCount { get; set; }

        public double DegreeOfOrganization { get; set; }

        public double Lambda { get; set; }

        public bool WithStartAndEnd { get; set; }
        public WorkingStationParameterSet GeneralWorkingStationParameterSet { get; set; }
        public WorkingStationParameterSet[] DetailedWorkingStationParameterSet { get; set; }

    }
}