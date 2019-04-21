using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore.Internal;
using Zpp.ModelExtensions;
using Zpp.Utils;
using Zpp.MrpRun;


namespace Zpp.CustomerManager
{
    public class CustomerManager
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private readonly ProductionDomainContext _productionDomainContext;

        public CustomerManager(ProductionDomainContext productionDomainContext)
        {
            _productionDomainContext = productionDomainContext;
        }

        public void Order(List<T_CustomerOrder> customerOrders)
        {
            T_CustomerOrder order = customerOrders[0];
            LOGGER.Info("Starting: with Order " + order.Id);

            M_Article rootArticle = order.OrderParts.ElementAt(0).Article;
            ITree<M_Article> articleTree =
                new ArticleTree(rootArticle, _productionDomainContext);
            TreeTools<M_Article>.traverseDepthFirst(articleTree, MrpRunAction);
        }

        private void MrpRunAction(Node<M_Article> articleNode)
        {
            switch (articleNode.Entity.ArticleType.Name)
            {
                case ArticleType.ASSEMBLY: MrpRun.Assembly(articleNode);
                    break;
                case ArticleType.CONSUMABLE: MrpRun.Consumable(articleNode);
                    break;
                case ArticleType.MATERIAL: MrpRun.Material(articleNode);
                    break;
                case ArticleType.PRODUCT: MrpRun.Product(articleNode);
                    break;
                default:
                    LOGGER.Error("Default case of switch articleType should not happen.");
                    break;
            }
        }


    }
}