using System;
using System.Collections.Generic;
using System.Text;
using Master40.PiWebApi.Interfaces;

namespace Master40.DB.DataModel
{
    public class M_Characteristic : BaseEntity, IPiWebCharacteristic
    {
        public string Name { get; set; }
        public int ArticleId { get; set; }
        public M_Article Article { get; set; }
        public int OperationId { get; set; }
        public M_Operation Operation { get; set; }
        public ICollection<M_Attribute> Attributes { get; set; }

        IPiWebArticle IPiWebCharacteristic.Article { get => this.Article; }
        IPiWebOperation IPiWebCharacteristic.Operation { get => this.Operation; }
        ICollection<IPiWebAttribute> IPiWebCharacteristic.Attributes { get => this.Attributes; }
    }
}
