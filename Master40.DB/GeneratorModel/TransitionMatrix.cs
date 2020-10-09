using System.ComponentModel.DataAnnotations.Schema;

namespace Master40.DB.GeneratorModel
{
    public class TransitionMatrix
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TransitionFrom { get; set; }
        public int TransitionTo { get; set; }
        public double Probability { get; set; }
        public int ApproachId { get; set; }
        public InputParameter Approach { get; set; }

    }
}