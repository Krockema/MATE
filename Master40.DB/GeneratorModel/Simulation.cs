using System.ComponentModel.DataAnnotations.Schema;

namespace Master40.DB.GeneratorModel
{
    public class Simulation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ApproachId { get; set; }
        public Approach Approach { get; set; }
    }
}