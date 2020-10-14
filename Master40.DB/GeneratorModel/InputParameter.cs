using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Master40.DB.DataModel;
using Newtonsoft.Json;

namespace Master40.DB.GeneratorModel
{
    public class InputParameter
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public bool ExtendedTransitionMatrix { get; set; }
        public double OrganizationDegree { get; set; }
        public DateTime CreationDate { get; set; }
        public virtual ICollection<TransitionMatrix> TransitionMatrix { get; set; }
    }
}