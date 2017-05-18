using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ArticleToBusinessPartner
    {
        public int ArticleId { get; set; }
        public int BusinessPartnerId { get; set; }
        public Article Article { get; set; }
        public BusinessPartner BusinessPartner { get; set; }
    }
}
