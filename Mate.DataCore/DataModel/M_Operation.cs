using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Mate.DataCore.Interfaces;
using Mate.PiWebApi.Interfaces;
using Newtonsoft.Json;

namespace Mate.DataCore.DataModel
{
    public class M_Operation : BaseEntity, IOperation, IPiWebOperation
    {
        public int HierarchyNumber { get; set; }
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        [NotMapped]
        public TimeSpan RandomizedDuration { get; set; }
        public TimeSpan AverageTransitionDuration { get; set; }
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        public int ResourceCapabilityId { get; set; }
        public M_ResourceCapability ResourceCapability { get; set; }
        public ICollection<M_ArticleBom> ArticleBoms { get; set; }
        public ICollection<M_Characteristic> Characteristics { get; set; }
        IEnumerable<IPiWebCharacteristic> IPiWebOperation.Characteristics => this.Characteristics;
    }
}