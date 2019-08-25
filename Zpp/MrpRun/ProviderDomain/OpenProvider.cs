using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;

namespace Zpp.ProviderDomain
{
    /**
     * Represents an provider, which quantity is not yet exceeded and can satisfy more demands.
     */
    public class OpenProvider
    {
        private readonly Provider _openProvider;
        private readonly Quantity _openQuantity;
        private readonly M_Article _article;

        public OpenProvider(Provider openProvider, Quantity openQuantity, M_Article article)
        {
            _openProvider = openProvider;
            _openQuantity = openQuantity;
            _article = article;
        }

        public Provider GetOpenProvider()
        {
            return _openProvider;
        }

        public Quantity GetOpenQuantity()
        {
            return _openQuantity;
        }

        public M_Article GetArticle()
        {
            return _article;
        }

        public override string ToString()
        {
            return $"{_openQuantity} open of '{_openProvider}'";
        }
    }
}