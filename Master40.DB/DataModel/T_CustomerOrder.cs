using System.Collections.Generic;
using Master40.DB.Nominal;
using Master40.DB.Interfaces;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class T_CustomerOrder : BaseEntity, IOrder
    {
        public string Name { get; set; }
        public int DueTime { get; set; }
        public int CreationTime { get; set; }
        public int FinishingTime { get; set; }
        public int BusinessPartnerId { get; set; }
        [JsonIgnore]
        public M_BusinessPartner BusinessPartner { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_CustomerOrderPart> CustomerOrderParts { get; set; }
        public State State { get; set; }

        public override string ToString()
        {
            return $"{Id}: {Name}; {DueTime}";
        }
    }
}