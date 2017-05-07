using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ArticleBomItem
    {
        [Key]
        public int ArticleBomItemId { get; set; }
        public int ArticleBomId { get; set; }

        public ArticleBom ArticleBom { get; set; }

        public int ArticleId { get; set; }
        public Article Article { get; set; }
        

        public decimal Quantity { get; set; }
        public string Name { get; set; }

    }
}
