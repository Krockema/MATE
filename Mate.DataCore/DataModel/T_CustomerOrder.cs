using System;
using System.Collections.Generic;
using Mate.DataCore.Interfaces;
using Mate.DataCore.Nominal;
using Newtonsoft.Json;

namespace Mate.DataCore.DataModel
{
    public class T_CustomerOrder : BaseEntity, IOrder
    {
        public string Name { get; set; }
        public DateTime DueTime { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime FinishingTime { get; set; }
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