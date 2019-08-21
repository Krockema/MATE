using System;
using System.Linq;
using Akka.Actor;
using Xunit;
using Akka.TestKit.Xunit;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using static IJobs;
using static FOperations;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.DataModel;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.Preparations;
using Microsoft.EntityFrameworkCore;

namespace Master40.XUnitTest.Agents.Types
{
    public class LimitedQueues : TestKit
    {
        public LimitedQueues()
        {
            
        }

        private FOperation CreateJobItem(string jobName, int jobDuration, bool preCondition = true , int dueTime = 50)
        {
            var operation = new M_Operation()
            {
                ArticleId = 10,
                AverageTransitionDuration = 20,
                Duration = jobDuration,
                HierarchyNumber = 10,
                Id = 1,
                Name = jobName

            };
            return operation.ToOperationItem(dueTime: 50, productionAgent: ActorRefs.Nobody, firstOperation: preCondition, currentTime: 0);
        }


        [Fact]
        public void AddToTimeLimitedQueue()
        {
            var jobQueueTimeLimited = new JobQueueTimeLimited(limit: 15);
            var addItemStatus = jobQueueTimeLimited.Enqueue(item: CreateJobItem(jobName: "Sample Operation 1", jobDuration: 10));
            Assert.True(condition: addItemStatus);
            
            addItemStatus = jobQueueTimeLimited.Enqueue(item: CreateJobItem(jobName: "Sample Operation 2", jobDuration: 5));
            Assert.True(condition: addItemStatus);

            addItemStatus = jobQueueTimeLimited.Enqueue(item: CreateJobItem(jobName: "Sample Operation 3", jobDuration: 10));
            Assert.False(condition: addItemStatus);

        }

        [Fact]
        public void AddToItemLimitedQueue()
        {
            var jobQueueItemLimited = new JobQueueItemLimited(limit: 1);
            var firstJob = CreateJobItem(jobName: "Sample Operation 1", jobDuration: 10);
            var addItemStatus = jobQueueItemLimited.Enqueue(item: firstJob);
            Assert.True(condition: addItemStatus);

            var secondJob = CreateJobItem(jobName: "Sample Operation 2", jobDuration: 20);
            addItemStatus = jobQueueItemLimited.Enqueue(item: secondJob);
            Assert.False(condition: addItemStatus);

        }

        [Theory]
        [InlineData(10, 50, 5, 35, 20, "SampleOne")]
        [InlineData(5, 35, 10, 50, 20, "SampleOne")]
        public void DequeueFromTimeLimitedQueue(int durationItemOne , int dueTimeItemOne, int durationItemTwo, int dueTimeItemTwo, int currentTime, string expected)
        {
            var jobQueueTimeLimited = new JobQueueTimeLimited(limit: 15);
            var operation1 = CreateJobItem(jobName: "SampleOne", jobDuration: durationItemOne, dueTime: dueTimeItemOne);
            operation1.StartConditions.ArticlesProvided = true;

            jobQueueTimeLimited.Enqueue(item: operation1);

            var operation2 = CreateJobItem(jobName: "SampleTwo", jobDuration: durationItemTwo, dueTime: dueTimeItemTwo);
            jobQueueTimeLimited.Enqueue(item: operation2);

            Assert.Equal(expected: 2, actual: jobQueueTimeLimited.Count);

            var dequeuedItem = jobQueueTimeLimited.DequeueFirstSatisfied(currentTime: currentTime);

            Assert.Equal(expected: expected, actual: ((FOperation)dequeuedItem).Operation.Name);
            Assert.Equal(expected: 1, actual: jobQueueTimeLimited.Count);

        }


    }
}
