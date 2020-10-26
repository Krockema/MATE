using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Master40.DB.Interfaces;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_Operation : BaseEntity, IOperation
    {
        public int HierarchyNumber { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        [NotMapped]
        public long RandomizedDuration { get; set; }
        public int AverageTransitionDuration { get; set; }
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        public int ResourceCapabilityId { get; set; }
        public M_ResourceCapability ResourceCapability { get; set; }
        public ICollection<M_ArticleBom> ArticleBoms { get; set; }
        public ICollection<M_Characteristic> Characteristics { get; set; }
    }
}