namespace Master40.DataGenerator.DataModel.TransitionMatrix
{
    public class InputParameterSet
    {

        public double DegreeOfOrganization { get; set; }

        public double Lambda { get; set; }

        public bool WithStartAndEnd { get; set; }
        public MachiningTimeParameterSet GeneralMachiningTimeParameterSet { get; set; }
        public WorkingStationParameterSet[] WorkingStations { get; set; }
        public bool InfiniteTools { get; set; }

    }
}