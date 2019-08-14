namespace Master40.SimulationCore.Agents.SupervisorAgent.Types
{
    public class OrderCounter
    {
        private int orders = 0;
        private int finishedOrders = 0;
        public int Max { get; private set; } = 0;
        public OrderCounter(int maxQuantity)
        {
            Max = maxQuantity;
        }
        public bool TryAddOne()
        {
            if (orders >= Max)
                return false;
            orders++;
            return true;
        }

        public int ProvidedOrder()
        {
            finishedOrders++;
            return finishedOrders;
        }
    }
}
