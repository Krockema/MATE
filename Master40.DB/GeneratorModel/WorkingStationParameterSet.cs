using System.ComponentModel.DataAnnotations.Schema;
using Master40.DB.Data.DynamicInitializer;

namespace Master40.DB.GeneratorModel
{
    public class WorkingStationParameterSet : ResourceProperty
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? MachiningTimeId { get; set; }
        public MachiningTimeParameterSet MachiningTimeParameterSet { get; set; }
        public int TransitionMatrixInputId { get; set; }
        public TransitionMatrixInput TransitionMatrixInput { get; set; }

    }
}