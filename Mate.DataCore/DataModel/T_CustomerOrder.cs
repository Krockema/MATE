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
        public int KiDueTime { get; set; }
        public int DueTime { get; set; }
        public DateTime DueDateTime { get; set; }
        public int CreationTime { get; set; }
        public long ReleaseTime { get; set; }
        public long KiReleaseTime { get; set; }
        public int FinishingTime { get; set; }
        public long TotalProcessingDuration { get; set; }
        public long LongestPathProcessingDuration { get; set; }
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