using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Master40.DB.DataModel
{
    public class M_Resource : BaseEntity
    {
        public string Name { get; set; }
        public bool IsPhysical { get; set; }
        public bool IsBiological { get; set; }
        public string GroupName { get; set; }
        public virtual ICollection<M_ResourceSetup> ResourceSetups { get; set; }
        public int Capacity { get; set; }
        [NotMapped]
        public object IResourceRef { get; set; }
        public override string ToString()
        {
            return Name;
        }
        public double Quantile { get; set; }
        public int NumberOfUses { get; set; }
    }
}