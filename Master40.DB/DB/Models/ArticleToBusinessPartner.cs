
namespace Master40.DB.DB.Models
{
    public class ArticleToBusinessPartner : BaseEntity
    {
        public int ArticleId { get; set; }
        public int BusinessPartnerId { get; set; }
        public Article Article { get; set; }
        public BusinessPartner BusinessPartner { get; set; }
        public int PackSize { get; set; }
        public int DueTime { get; set; }
        public double Price { get; set; }
    }
}
