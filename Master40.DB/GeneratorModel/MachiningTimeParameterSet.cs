using System.ComponentModel.DataAnnotations.Schema;

namespace Master40.DB.GeneratorModel
{
    public class MachiningTimeParameterSet
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public double MeanMachiningTime { get; set; }
        public double VarianceMachiningTime { get; set; }
        public WorkingStationParameterSet WorkingStation { get; set; }
        public TransitionMatrixInput TransitionMatrix { get; set; }
    }
}