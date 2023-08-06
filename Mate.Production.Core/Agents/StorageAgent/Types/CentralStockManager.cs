using Mate.Production.Core.Environment.Records.Central;
using System;

namespace Mate.Production.Core.Agents.StorageAgent.Types
{
    public class CentralStockManager
    {
        private CentralStockDefinitionRecord _stockDefinition { get; }

        public double _quantity { get; private set; }
        public CentralStockManager(CentralStockDefinitionRecord stockDefinition)
        {
            _stockDefinition = stockDefinition;
            _quantity = stockDefinition.InitialQuantity;

        }

        public void Add(double quantity)
        {
            _quantity += quantity;
        }

        public void Remove(double quantity)
        {
            _quantity -= quantity;
        }

        public string MaterialType => _stockDefinition.MaterialType;
        public string MaterialName => _stockDefinition.MaterialName;
        public TimeSpan DeliveryPeriod => _stockDefinition.DeliveryPeriod;
        public double Value => Convert.ToDouble(value: _quantity) * Convert.ToDouble(value: _stockDefinition.Price);
    }
}
