
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class ArticleToBusinessPartner : BaseEntity
    {
        public const string BUSINESSPARTNER_FKEY = "BusinessPartner";
        
        public int ArticleId { get; set; }
        public int BusinessPartnerId { get; set; }
        [JsonIgnore]
        public Article Article { get; set; }
        [JsonIgnore]
        public BusinessPartner BusinessPartner { get; set; }
        public int PackSize { get; set; }
        public int DueTime { get; set; }
        public double Price { get; set; }
    }
}
