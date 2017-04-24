using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ArticleBom
    {
        public int ArticleBomId { get; set; }
        public string Name { get; set; }
        public ICollection<ArticleBomPart> ArticleBomParts { get; set; }
        public int? ArticleId { get; set; }
        public Article Article { get; set; }
    }
}
