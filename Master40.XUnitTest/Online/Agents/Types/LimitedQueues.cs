using Akka.Actor;
using Akka.TestKit.Xunit;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.XUnitTest.Online.Preparations;
using System.Collections.Generic;
using Xunit;
using static FJobConfirmations;
using static FOperations;

namespace Master40.XUnitTest.Online.Agents.Types
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

            var jobConfirmation = new FJobConfirmation(TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 1", jobDuration: 10), 0, 10,
                new FSetupDefinitions.FSetupDefinition(0, new List<IActorRef>()));

            jobQueueTimeLimited.Enqueue(jobConfirmation);
            Assert.True(condition: jobQueueTimeLimited.Count == 1);

            var jobConfirmation2 = new FJobConfirmation(TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 2", jobDuration: 5), 10, 5,
                new FSetupDefinitions.FSetupDefinition(0, new List<IActorRef>()));
            
            jobQueueTimeLimited.Enqueue(jobConfirmation2);
            Assert.True(jobQueueTimeLimited.Count == 2);

            //addItemStatus = jobQueueTimeLimited.Enqueue(item: TypeFactory.CreateJobItem(jobName: "Sample Operation 3", jobDuration: 10));
            //Assert.False(condition: addItemStatus);

        }

        [Fact]
        public void AddToItemLimitedQueue()
        {
            var jobQueueItemLimited = new JobQueueItemLimited(limit: 1);
            var jobConfirmation1 = new FJobConfirmation(TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 1", jobDuration: 10), 0, 10,
                new FSetupDefinitions.FSetupDefinition(0, new List<IActorRef>()));
            var addItemStatus = jobQueueItemLimited.Enqueue(jobConfirmation1);
            Assert.True(condition: addItemStatus);

            var jobConfirmation2 = new FJobConfirmation(TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 1", jobDuration: 20), 10, 20,
                new FSetupDefinitions.FSetupDefinition(0, new List<IActorRef>()));
            addItemStatus = jobQueueItemLimited.Enqueue(jobConfirmation2);
            Assert.False(condition: addItemStatus);

        }

        [Theory]
        [InlineData(10, 50, 5, 35, 20, "SampleOne")]
        [InlineData(5, 35, 10, 50, 20, "SampleOne")]
        public void DequeueFromTimeLimitedQueue(int durationItemOne , int dueTimeItemOne, int durationItemTwo, int dueTimeItemTwo, int currentTime, string expected)
        {
            var jobQueueTimeLimited = new JobQueueTimeLimited(limit: 15);
            var jobConfirmation1 = new FJobConfirmation(TypeFactory.CreateDummyJobItem(jobName: "SampleOne", jobDuration: durationItemOne, dueTime: dueTimeItemOne), 0, durationItemOne,
                new FSetupDefinitions.FSetupDefinition(0, new List<IActorRef>()));
            jobConfirmation1.Job.StartConditions.ArticlesProvided = true;

            jobQueueTimeLimited.Enqueue(jobConfirmation1);

            var jobConfirmation2 = new FJobConfirmation(TypeFactory.CreateDummyJobItem(jobName: "SampleTwo", jobDuration: durationItemTwo, dueTime: dueTimeItemTwo), 10, durationItemOne,
                new FSetupDefinitions.FSetupDefinition(0, new List<IActorRef>()));
            jobQueueTimeLimited.Enqueue(jobConfirmation2);

            Assert.Equal(expected: 2, actual: jobQueueTimeLimited.Count);

            var dequeuedItem = jobQueueTimeLimited.DequeueFirstSatisfied(currentTime: currentTime);

            Assert.Equal(expected: expected, actual: ((FOperation)dequeuedItem.Job).Operation.Name);
            Assert.Equal(expected: 1, actual: jobQueueTimeLimited.Count);

        }

    }
}
