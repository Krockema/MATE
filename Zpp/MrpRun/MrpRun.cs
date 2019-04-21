using Master40.DB.DataModel;
using Zpp.ModelExtensions;
using Zpp.Utils;

namespace Zpp.MrpRun
{
    public static class MrpRun
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        
        public static void Assembly(Node<M_Article> articleNode)
        {
            if (!articleNode.Entity.ArticleType.Name.Equals(ArticleType.ASSEMBLY))
            {
                LOGGER.Error("ArticleType is not suited for MrpRun." + ArticleType.ASSEMBLY +"()");
            }
        }
        
        public static void Consumable(Node<M_Article> articleNode)
        {
            if (!articleNode.Entity.ArticleType.Name.Equals(ArticleType.CONSUMABLE))
            {
                LOGGER.Error("ArticleType is not suited for MrpRun." + ArticleType.CONSUMABLE +"()");
            }
        }
        
        public static void Material(Node<M_Article> articleNode)
        {
            if (!articleNode.Entity.ArticleType.Name.Equals(ArticleType.MATERIAL))
            {
                LOGGER.Error("ArticleType is not suited for MrpRun." + ArticleType.MATERIAL +"()");
            }
        }
        
        public static void Product(Node<M_Article> articleNode)
        {
            if (!articleNode.Entity.ArticleType.Name.Equals(ArticleType.PRODUCT))
            {
                LOGGER.Error("ArticleType is not suited for MrpRun." + ArticleType.PRODUCT +"()");
            }
        }
    }
}