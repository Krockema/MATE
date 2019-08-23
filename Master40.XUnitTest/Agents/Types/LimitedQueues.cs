using Akka.Actor;
using Akka.TestKit.Xunit;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.Preparations;
using Xunit;
using static FOperations;

namespace Master40.XUnitTest.Agents.Types
{
    public class LimitedQueues : TestKit
    {
        public LimitedQueues()
        {
            
        }


        //TODO 
        [Fact]
        public void AddToTimeLimitedQueue()
        {
            var jobQueueTimeLimited = new JobQueueTimeLimited(limit: 15);
            jobQueueTimeLimited.Enqueue(item: TypeFactory.CreateJobItem(jobName: "Sample Operation 1", jobDuration: 10));
            Assert.True(condition: jobQueueTimeLimited.Count == 1);
            
            jobQueueTimeLimited.Enqueue(item: TypeFactory.CreateJobItem(jobName: "Sample Operation 2", jobDuration: 5));
            Assert.True(jobQueueTimeLimited.Count == 2);

            //addItemStatus = jobQueueTimeLimited.Enqueue(item: TypeFactory.CreateJobItem(jobName: "Sample Operation 3", jobDuration: 10));
            //Assert.False(condition: addItemStatus);

        }

        [Fact]
        public void AddToItemLimitedQueue()
        {
            var jobQueueItemLimited = new JobQueueItemLimited(limit: 1);
            var firstJob = TypeFactory.CreateJobItem(jobName: "Sample Operation 1", jobDuration: 10);
            var addItemStatus = jobQueueItemLimited.Enqueue(item: firstJob);
            Assert.True(condition: addItemStatus);

            var secondJob = TypeFactory.CreateJobItem(jobName: "Sample Operation 2", jobDuration: 20);
            addItemStatus = jobQueueItemLimited.Enqueue(item: secondJob);
            Assert.False(condition: addItemStatus);

        }

        [Theory]
        [InlineData(10, 50, 5, 35, 20, "SampleOne")]
        [InlineData(5, 35, 10, 50, 20, "SampleOne")]
        public void DequeueFromTimeLimitedQueue(int durationItemOne , int dueTimeItemOne, int durationItemTwo, int dueTimeItemTwo, int currentTime, string expected)
        {
            var jobQueueTimeLimited = new JobQueueTimeLimited(limit: 15);
            var operation1 = TypeFactory.CreateJobItem(jobName: "SampleOne", jobDuration: durationItemOne, dueTime: dueTimeItemOne);
            operation1.StartConditions.ArticlesProvided = true;

            jobQueueTimeLimited.Enqueue(item: operation1);

            var operation2 = TypeFactory.CreateJobItem(jobName: "SampleTwo", jobDuration: durationItemTwo, dueTime: dueTimeItemTwo);
            jobQueueTimeLimited.Enqueue(item: operation2);

            Assert.Equal(expected: 2, actual: jobQueueTimeLimited.Count);

            var dequeuedItem = jobQueueTimeLimited.DequeueFirstSatisfied(currentTime: currentTime);

            Assert.Equal(expected: expected, actual: ((FOperation)dequeuedItem).Operation.Name);
            Assert.Equal(expected: 1, actual: jobQueueTimeLimited.Count);

        }


    }
}
