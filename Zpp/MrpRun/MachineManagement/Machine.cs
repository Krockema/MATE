using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.WrappersForPrimitives;

namespace Zpp.MrpRun.MachineManagement
{
    public class Machine : IMachine
    {
        private M_Machine _machine;
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
            Machine other = (Machine) obj;
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