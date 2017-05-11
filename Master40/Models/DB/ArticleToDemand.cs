using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ArticleToDemand
    {
        public int ArticleId { get; set; }
        public int DemandToProviderId { get; set; }
        public Article Article { get; set; }
        public DemandToProvider DemandToProvider { get; set; }
        [Required]
        public decimal Quantity { get; set; }
    }
}
