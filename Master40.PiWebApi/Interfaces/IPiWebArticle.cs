using System.Collections.Generic;

namespace Master40.PiWebApi.Interfaces
{
    public interface IPiWebArticle
    {
        string Name { get; }
        IEnumerable<IPiWebOperation> GetPiWebOperations();
    }
}