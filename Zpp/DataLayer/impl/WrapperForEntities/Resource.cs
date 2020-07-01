using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;

namespace Zpp.DataLayer.impl.WrapperForEntities
{
    public class Resource : IResource
    {
        private readonly M_Resource _resource;
        private int _idleStartTime = 0;

        public Resource(M_Resource resource)
        {
            _resource = resource;
        }

        public M_Resource GetValue()
        {
            return _resource;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Resource;
            if (other?._resource == null)
                return false;
            return _resource.Equals(other._resource);
        }

        public override int GetHashCode()
        {
            return _resource.GetHashCode();
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