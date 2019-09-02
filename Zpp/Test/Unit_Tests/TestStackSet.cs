using Xunit;
using Zpp.MrpRun.MachineManagement;

namespace Zpp.Test.Unit_Tests
{
    public class TestStackSet
    {
        [Fact]
        public void TestAdd()
        {
            IStackSet<int> stackSet = new StackSet<int>();
            int testNumber = 1;
            stackSet.Push(testNumber);
            stackSet.Push(testNumber);
            Assert.True(stackSet.Count().Equals(testNumber), "Set contains a duplicate.");
            Assert.True(stackSet.PopAny().Equals(testNumber), "Value was not correctly added.");
        }

        [Fact]
        public void TestAny()
        {
            IStackSet<int> stackSet = new StackSet<int>();
            Assert.False(stackSet.Any(), "Set should not contain any.");

            int testNumber = 1;
            stackSet.Push(testNumber);
            Assert.True(stackSet.Any(), "Set should contain any.");
        }


        [Fact]
        public void TestRemove()
        {
            IStackSet<int> stackSet = new StackSet<int>();
            int testNumber = 1;
            int testNumber2 = 5;
            stackSet.Push(testNumber);
            stackSet.Push(testNumber2);
            stackSet.Remove(testNumber);
            Assert.True(stackSet.GetAny().Equals(testNumber2) && stackSet.Count() == 1,
                "Remove didn't work.");
            stackSet.Remove(testNumber2);
            Assert.True(stackSet.Any() == false && stackSet.Count() == 0,
                "Remove didn't work.");
        }

        [Fact]
        public void TestPopAny()
        {
            IStackSet<int> stackSet = new StackSet<int>();
            int testNumber = 1;
            int testNumber2 = 5;
            stackSet.Push(testNumber);
            stackSet.Push(testNumber2);
            int poppedElement = stackSet.PopAny();
            Assert.True(
                (poppedElement.Equals(testNumber2) || poppedElement.Equals(testNumber)) &&
                stackSet.Count() == 1, "PopAny didn't work.");
        }

        [Fact]
        public void TestGetAny()
        {
            IStackSet<int> stackSet = new StackSet<int>();
            int testNumber = 1;
            int testNumber2 = 5;
            stackSet.Push(testNumber);
            stackSet.Push(testNumber2);
            int poppedElement = stackSet.GetAny();
            Assert.True(
                (poppedElement.Equals(testNumber2) || poppedElement.Equals(testNumber)) &&
                stackSet.Count() == 2, "PopAny didn't work.");
        }

        [Fact]
        public void TestCount()
        {
            IStackSet<int> stackSet = new StackSet<int>();
            int testNumber = 1;
            int testNumber2 = 5;
            stackSet.Push(testNumber);
            stackSet.Push(testNumber2);
            Assert.True(
                stackSet.Count().Equals(2), "PopAny didn't work.");
        }
    }
}