using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.DataModel
{
    public class M_Characteristic : BaseEntity
    {
        public string Name { get; set; }
        public int ArticleId { get; set; }
        public M_Article Article { get; set; }
        public int OperationId { get; set; }
        public M_Operation Operation { get; set; }
        public ICollection<M_Attribute> Attributes { get; set; }
    }
}
