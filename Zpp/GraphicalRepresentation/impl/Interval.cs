using Master40.DB.Data.WrappersForPrimitives;

namespace Zpp.GraphicalRepresentation.impl
{
    public class Interval
    {
        private readonly Id _id;
        private readonly DueTime _start;
        private readonly DueTime _end;

        public Interval(Id id, DueTime start, DueTime end)
        {
            _id = id;
            _start = start;
            _end = end;
        }

        public bool IntersectsExclusive(Interval other)
        {
            return _start.IsSmallerThan(other._end) && other._start.IsSmallerThan(_end);
        }
    }
}