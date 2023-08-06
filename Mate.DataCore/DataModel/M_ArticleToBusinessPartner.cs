
using Newtonsoft.Json;
using System;

namespace Mate.DataCore.DataModel
{
    public class M_ArticleToBusinessPartner : BaseEntity
    {
        public int ArticleId { get; set; }
        public int BusinessPartnerId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        [JsonIgnore]
        public M_BusinessPartner BusinessPartner { get; set; }
        public int PackSize { get; set; }
        public TimeSpan TimeToDelivery { get; set; }
        public double Price { get; set; }
    }
}
