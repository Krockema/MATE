namespace Zpp.WrappersForPrimitives
{
    public class HierarchyNumber
    {
        private int _hierarchyNumber;

        public HierarchyNumber(int hierarchyNumber)
        {
            _hierarchyNumber = hierarchyNumber;
        }

        public void increment()
        {
            _hierarchyNumber++;
        }

        public int GetValue()
        {
            return _hierarchyNumber;
        }

        public override bool Equals(object obj)
        {
            HierarchyNumber other = (HierarchyNumber) obj;
            return _hierarchyNumber.Equals(other._hierarchyNumber);
        }

        public override int GetHashCode()
        {
            return _hierarchyNumber.GetHashCode();
        }

        public override string ToString()
        {
            return _hierarchyNumber.ToString();
        }

        public bool IsGreaterThan(HierarchyNumber other)
        {
            return _hierarchyNumber > other._hierarchyNumber;
        }
        
        public bool IsSmallerThan(HierarchyNumber other)
        {
            return _hierarchyNumber < other._hierarchyNumber;
        }
        
        public bool IsGreaterThanOrEquals(HierarchyNumber other)
        {
            return _hierarchyNumber >= other._hierarchyNumber;
        }
    }
}