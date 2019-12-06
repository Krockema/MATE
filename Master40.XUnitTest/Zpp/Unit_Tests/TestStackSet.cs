using Master40.DB.Data.WrappersForPrimitives;
using Xunit;
using Zpp.Util.Graph;
using Zpp.Util.StackSet;

namespace Master40.XUnitTest.Zpp.Unit_Tests
{
    public class TestStackSet
    {
        [Fact]
        public void TestAdd()
        {
            IStackSet<IScheduleNode> stackSet = new StackSet<IScheduleNode>();
            IScheduleNode testNumber = new DummyNode(1);
            stackSet.Push(testNumber);
            stackSet.Push(testNumber);
            Assert.True(stackSet.Count().Equals(1), "Set contains a duplicate.");
            Assert.True(stackSet.PopAny().Equals(new DummyNode(new Id(1))),
                "Value was not correctly added.");
        }

        [Fact]
        public void TestAny()
        {
            IStackSet<IScheduleNode> stackSet = new StackSet<IScheduleNode>();
            Assert.False(stackSet.Any(), "Set should not contain any.");

            IScheduleNode testNumber = new DummyNode(1);
            stackSet.Push(testNumber);
            Assert.True(stackSet.Any(), "Set should contain any.");
        }


        [Fact]
        public void TestRemove()
        {
            IStackSet<IScheduleNode> stackSet = new StackSet<IScheduleNode>();
            IScheduleNode testNumber = new DummyNode(1);
            IScheduleNode testNumber2 = new DummyNode(5);
            stackSet.Push(testNumber);
            stackSet.Push(testNumber2);
            stackSet.Remove(testNumber);
            Assert.True(stackSet.GetAny().Equals(testNumber2) && stackSet.Count() == 1,
                "Remove didn't work.");
            stackSet.Remove(testNumber2);
            Assert.True(stackSet.Any() == false && stackSet.Count() == 0, "Remove didn't work.");
        }

        [Fact]
        public void TestPopAny()
        {
            IStackSet<IScheduleNode> stackSet = new StackSet<IScheduleNode>();
            IScheduleNode testNumber = new DummyNode(1);
            IScheduleNode testNumber2 = new DummyNode(5);
            stackSet.Push(testNumber);
            stackSet.Push(testNumber2);
            IScheduleNode poppedElement = stackSet.PopAny();
            Assert.True(
                (poppedElement.Equals(testNumber2) || poppedElement.Equals(testNumber)) &&
                stackSet.Count() == 1, "PopAny didn't work.");
        }

        [Fact]
        public void TestGetAny()
        {
            IStackSet<IScheduleNode> stackSet = new StackSet<IScheduleNode>();
            IScheduleNode testNumber = new DummyNode(1);
            IScheduleNode testNumber2 = new DummyNode(5);
            stackSet.Push(testNumber);
            stackSet.Push(testNumber2);
            IScheduleNode poppedElement = stackSet.GetAny();
            Assert.True(
                (poppedElement.Equals(testNumber2) || poppedElement.Equals(testNumber)) &&
                stackSet.Count() == 2, "PopAny didn't work.");
        }

        [Fact]
        public void TestCount()
        {
            IStackSet<IScheduleNode> stackSet = new StackSet<IScheduleNode>();
            IScheduleNode testNumber = new DummyNode(1);
            IScheduleNode testNumber2 = new DummyNode(5);
            stackSet.Push(testNumber);
            stackSet.Push(testNumber2);
            Assert.True(stackSet.Count().Equals(2), "PopAny didn't work.");
        }

        [Fact]
        public void TestGetById()
        {
            IStackSet<IScheduleNode> stackSet = new StackSet<IScheduleNode>();
            int countNodes = 10;
            for (int i = 0; i < countNodes; i++)
            {
                IScheduleNode testNumber = new DummyNode(i);
                stackSet.Push(testNumber);
            }

            for (int i = countNodes - 1; i > 0; i--)
            {
                IScheduleNode scheduleNode = new DummyNode(i);
                Assert.True(
                    stackSet.GetById(new Id(i)).Equals(scheduleNode) 
                    && stackSet.Count() == i + 1,
                    "GetById() didn't work.");
                stackSet.Remove(scheduleNode);
            }
        }
        
        [Fact]
        public void TestContains()
        {
            IStackSet<IScheduleNode> stackSet = new StackSet<IScheduleNode>();
            int countNodes = 10;
            for (int i = 0; i < countNodes; i++)
            {
                IScheduleNode testNumber = new DummyNode(i);
                stackSet.Push(testNumber);
            }

            for (int i = countNodes - 1; i > 0; i--)
            {
                IScheduleNode scheduleNode = new DummyNode(i);
                Assert.True(
                    stackSet.Contains(scheduleNode) 
                    && stackSet.Count() == i + 1,
                    "Contains() didn't work.");
                stackSet.Remove(scheduleNode);
            }
        }
    }
}