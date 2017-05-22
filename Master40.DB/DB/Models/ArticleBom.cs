using System.ComponentModel.DataAnnotations;

namespace Master40.DB.Models
{
    public class ArticleBom : BaseEntity
    {

        public int? ArticleParentId { get; set; }
        public Article ArticleParent { get; set; }
        public int ArticleChildId { get; set; }
        public Article ArticleChild { get; set; }

        public decimal Quantity { get; set; }
        public string Name { get; set; }

    }
}
