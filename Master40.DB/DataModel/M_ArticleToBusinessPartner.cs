
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_ArticleToBusinessPartner : BaseEntity
    {
        public const string BUSINESSPARTNER_FKEY = "BusinessPartner";
        
        public int ArticleId { get; set; }
        public int BusinessPartnerId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        [JsonIgnore]
        public M_BusinessPartner BusinessPartner { get; set; }
        public int PackSize { get; set; }
        public int DueTime { get; set; }
        public double Price { get; set; }
    }
}
