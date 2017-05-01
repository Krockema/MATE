using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ArticleBom
    {
        /*  Article Bom with M to N Relation /*
        public int ArticleBomPartId { get; set; }
        public ArticleBomPart ArticleBomPart { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        */ 
        [Key]
        public int ArticleBomId { get; set; }
        public int ArticleParentId { get; set; }
        public Article ArticleParent { get; set; }
        public int? ArticleChildId { get; set; }
        public Article ArticleChild { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }

    }
}
