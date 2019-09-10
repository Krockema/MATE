using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.WrappersForPrimitives;

namespace Zpp.Mrp.MachineManagement
{
    public class Resource : IResource
    {
        private readonly M_Resource _machine;
        private int _idleStartTime = 0;

        public Resource(M_Resource machine)
        {
            _machine = machine;
        }

        public M_Resource GetValue()
        {
            return _machine;
        }
        /// <summary>
        /// Depricated 
        /// </summary>
        /// <returns></returns>
        public Id GetFirstMachineSkillId()
        {
            return new Id(_machine.ResourceSkills.First().Id);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Resource;
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