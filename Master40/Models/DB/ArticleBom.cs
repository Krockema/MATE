using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ArticleBom
    {
        [Key]
        public int ArticleBomId { get; set; }
        public int? ArticleParentId { get; set; }
        public Article ArticleParent { get; set; }
        /// <summary>
        /// current Artilcle.
        /// </summary>
        public int? ArticleChildId { get; set; }
        public Article ArticleChild { get; set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; }

    }
}
