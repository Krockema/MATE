using Xunit;
using Mate.Production.Core;

namespace Mate.Test.Online.Agents.Types
{
    public class OrderCounter
    {
        Mate.Production.Core.Agents.SupervisorAgent.Types.OrderCounter _orderCounter = new (maxQuantity: 2);
        
        [Fact]
        public void AddOrder()
        {
            bool addNewOrderSuccessful = _orderCounter.TryAddOne();
            Assert.True(condition: addNewOrderSuccessful);
        }


        [Fact]
        public void AddOrderOverMax()
        {
            //add 2
            _orderCounter.TryAddOne();
            bool addNewOrderSuccessful = _orderCounter.TryAddOne();
            Assert.True(condition: addNewOrderSuccessful);
            //add more than 2
            bool addNewOrderOverMaxExeeded = _orderCounter.TryAddOne();
            Assert.False(condition: addNewOrderOverMaxExeeded);
        }
    }
}
