using System.ComponentModel.DataAnnotations.Schema;

namespace Master40.DB.GeneratorModel
{
    public class BillOfMaterialInput
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public bool RoundEdgeWeight { get; set; }
        public decimal WeightEpsilon { get; set; }
        public int ApproachId { get; set; }
        public Approach Approach { get; set; }
    }
}