using Xunit;
using Zpp.MachineDomain;

namespace Zpp.Test
{
    public class TestSet
    {
        [Fact]
        public void TestAdd()
        {
            ISet<int> set = new Set<int>();
            int testNumber = 1;
            set.Add(testNumber);
            set.Add(testNumber);
            Assert.True(set.Count().Equals(testNumber), "Set contains a duplicate.");
            Assert.True(set.PopAny().Equals(testNumber), "Value was not correctly added.");
        }

        [Fact]
        public void TestAny()
        {
            ISet<int> set = new Set<int>();
            Assert.False(set.Any(), "Set should not contain any.");

            int testNumber = 1;
            set.Add(testNumber);
            Assert.True(set.Any(), "Set should contain any.");
        }


        [Fact]
        public void TestRemove()
        {
            ISet<int> set = new Set<int>();
            int testNumber = 1;
            int testNumber2 = 5;
            set.Add(testNumber);
            set.Add(testNumber2);
            set.Remove(testNumber);
            Assert.True(set.GetAny().Equals(testNumber2) && set.Count() == 1,
                "Remove didn't work.");
        }

        [Fact]
        public void TestPopAny()
        {
            ISet<int> set = new Set<int>();
            int testNumber = 1;
            int testNumber2 = 5;
            set.Add(testNumber);
            set.Add(testNumber2);
            int poppedElement = set.PopAny();
            Assert.True(
                (poppedElement.Equals(testNumber2) || poppedElement.Equals(testNumber)) &&
                set.Count() == 1, "PopAny didn't work.");
        }

        [Fact]
        public void TestGetAny()
        {
            ISet<int> set = new Set<int>();
            int testNumber = 1;
            int testNumber2 = 5;
            set.Add(testNumber);
            set.Add(testNumber2);
            int poppedElement = set.GetAny();
            Assert.True(
                (poppedElement.Equals(testNumber2) || poppedElement.Equals(testNumber)) &&
                set.Count() == 2, "PopAny didn't work.");
        }

        [Fact]
        public void TestCount()
        {
            ISet<int> set = new Set<int>();
            int testNumber = 1;
            int testNumber2 = 5;
            set.Add(testNumber);
            set.Add(testNumber2);
            Assert.True(
                set.Count().Equals(2), "PopAny didn't work.");
        }
    }
}