using System.Collections.Generic;

namespace Master40.PiWebApi.Interfaces
{
    public interface IPiWebCharacteristic
    {
        public string Name { get; }
        public int ArticleId { get; }
        public IPiWebArticle Article { get; }
        public int OperationId { get; }
        public IPiWebOperation Operation { get;  }
        public IEnumerable<IPiWebAttribute> Attributes { get; }
    }
}