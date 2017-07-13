using Newtonsoft.Json;

namespace Master40.DB.Models
{
    public class ArticleBom : BaseEntity
    {

        public int? ArticleParentId { get; set; }
        [JsonIgnore]
        public Article ArticleParent { get; set; }
        public int ArticleChildId { get; set; }
        [JsonIgnore]
        public Article ArticleChild { get; set; }

        public decimal Quantity { get; set; }
        public string Name { get; set; }

    }
}
