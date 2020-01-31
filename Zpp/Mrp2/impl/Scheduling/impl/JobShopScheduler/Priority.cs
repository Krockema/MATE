using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp.Mrp2.impl.Scheduling.impl.JobShopScheduler
{
    public class Priority: IntPrimitive<Priority>
    {
        public Priority(int @int) : base(@int)
        {
        }

        public Priority() : base()
        {
        }
    }
}