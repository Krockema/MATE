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
    }
}