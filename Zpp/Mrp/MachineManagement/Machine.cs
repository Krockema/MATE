using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.WrappersForPrimitives;

namespace Zpp.Mrp.MachineManagement
{
    public class Machine : IMachine
    {
        private readonly M_Machine _machine;
        private int _idleStartTime = 0;

        public Machine(M_Machine machine)
        {
            _machine = machine;
        }

        public M_Machine GetValue()
        {
            return _machine;
        }

        public Id GetMachineGroupId()
        {
            return new Id(_machine.MachineGroupId);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Machine;
            if (other?._machine == null)
                return false;
            return _machine.Equals(other._machine);
        }

        public override int GetHashCode()
        {
            return _machine.GetHashCode();
        }

        public DueTime GetIdleStartTime()
        {
            return new DueTime(_idleStartTime);
        }

        public void SetIdleStartTime(DueTime nextIdleStartTime)
        {
            _idleStartTime = nextIdleStartTime.GetValue();
        }
    }
}