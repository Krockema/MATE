using System.Collections.Generic;
using Master40.DB.Enums;
using Newtonsoft.Json;

namespace Master40.DB.Models
{
    public class Order : BaseEntity
    {
        public string Name { get; set; }
        public int DueTime { get; set; }
        public int CreationTime { get; set; }
        public int FinishingTime { get; set; }
        public int BusinessPartnerId { get; set; }
        [JsonIgnore]
        public BusinessPartner BusinessPartner { get; set; }
        [JsonIgnore]
        public virtual ICollection<OrderPart> OrderParts { get; set; }
        public State State { get; set; }
    }
}
