using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ArticleToDemand
    {
        public int ArticleToDemandId { get; set; }
        public int ArticleId { get; set; }
        public int DemandId { get; set; }
        public Article Article { get; set; }
        public Demand Demand { get; set; }
    }
}
