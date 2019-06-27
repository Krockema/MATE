using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.WrappersForPrimitives;
using Master40.DB.DataModel;

namespace Zpp
{
    public class DbCacheMasterData : IDbCacheMasterData
    {
        private readonly ProductionDomainContext _productionDomainContext;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        // cached tables
        // M_*
        
        private readonly List<M_Article> _articles;
        private readonly List<M_ArticleBom> _articleBoms;
        private readonly List<M_ArticleToBusinessPartner> _articleToBusinessPartners;
        private readonly List<M_ArticleType> _articleTypes;
        private readonly List<M_BusinessPartner> _businessPartners;
        private readonly List<M_Machine> _machines;
        private readonly List<M_MachineGroup> _machineGroups;
        private readonly List<M_MachineTool> _machineTools;
        private readonly List<M_Operation> _operations;
        private readonly List<M_Stock> _stocks;
        private readonly List<M_Unit> _units;
        
        public DbCacheMasterData(ProductionDomainContext productionDomainContext)
        {
            _productionDomainContext = productionDomainContext;
            
            // cache tables
            _articles = _productionDomainContext.Articles.ToList();
            _articleBoms = _productionDomainContext.ArticleBoms.ToList();
            _articleToBusinessPartners = 
                _productionDomainContext.ArticleToBusinessPartners.ToList();
            _articleTypes = _productionDomainContext.ArticleTypes.ToList();
            _businessPartners = _productionDomainContext.BusinessPartners.ToList();
            _machines = _productionDomainContext.Machines.ToList();
            _machineGroups = _productionDomainContext.MachineGroups.ToList();
            _machineTools = _productionDomainContext.MachineTools.ToList();
            _operations = _productionDomainContext.Operations.ToList();
            _stocks = _productionDomainContext.Stocks.ToList();
            _units = _productionDomainContext.Units.ToList();
        }
        
        public List<M_BusinessPartner> M_BusinessPartnerGetAll()
        {
            return _businessPartners;
        }

        public M_ArticleBom M_ArticleBomGetById(Id id)
        {
            return _articleBoms.Single(x => x.Id == id.GetValue());
        }

        public M_Article M_ArticleGetById(Id id)
        {
            return _articles.Single(x => x.Id == id.GetValue());
        }

        public M_ArticleToBusinessPartner M_ArticleToBusinessPartnerGetById(Id id)
        {
            return _articleToBusinessPartners.Single(x => x.Id == id.GetValue());
        }

        public M_ArticleType M_ArticleTypeGetById(Id id)
        {
            return _articleTypes.Single(x => x.Id == id.GetValue());
        }

        public M_Machine M_MachineGetById(Id id)
        {
            return _machines.Single(x => x.Id == id.GetValue());
        }

        public M_MachineGroup M_MachineGroupGetById(Id id)
        {
            return _machineGroups.Single(x => x.Id == id.GetValue());
        }

        public M_MachineTool M_MachineToolGetById(Id id)
        {
            return _machineTools.Single(x => x.Id == id.GetValue());
        }

        public M_Operation M_OperationGetById(Id id)
        {
            return _operations.Single(x => x.Id == id.GetValue());
        }

        public M_Stock M_StockGetById(Id id)
        {
            return _stocks.Single(x => x.Id == id.GetValue());
        }

        public M_Unit M_UnitGetById(Id id)
        {
            return _units.Single(x => x.Id == id.GetValue());
        }
    }
}