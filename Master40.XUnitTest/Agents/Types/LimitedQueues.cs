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

        private FOperation CreateJobItem(string jobName, int jobDuration, int dueTime = 50)
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
            return operation.ToOperationItem(dueTime: 50, productionAgent: ActorRefs.Nobody, firstOperation: false, currentTime: 0);
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
            var addItemStatus = jobQueueItemLimited.Enqueue(item: CreateJobItem(jobName: "Sample Operation 1", jobDuration: 10));
            Assert.True(condition: addItemStatus);

            addItemStatus = jobQueueItemLimited.Enqueue(item: CreateJobItem(jobName: "Sample Operation 2", jobDuration: 20));
            Assert.False(condition: addItemStatus);
        }

        [Theory]
        [InlineData(10, 50, 5, 35, 20, "SampleOne")]
        public void DequeueFromLimitedQueue(int durationItemOne , int dueTimeItemOne, int durationItemTwo, int dueTimeItemTwo, int currentTime, string expected)
        {
            var jobQueueTimeLimited = new JobQueueTimeLimited(limit: 15);
            jobQueueTimeLimited.Enqueue(item: CreateJobItem(jobName: "SampleOne", jobDuration: durationItemOne, dueTime: dueTimeItemOne));

            jobQueueTimeLimited.Enqueue(item: CreateJobItem(jobName: "SampleTwo", jobDuration: durationItemTwo, dueTime: dueTimeItemTwo));

            var dequeuedItem = jobQueueTimeLimited.Dequeue(currentTime: currentTime);
            Assert.Equal(expected: expected, actual: ((FOperation)dequeuedItem).Operation.Name);

            Assert.Equal(expected: 1, actual: jobQueueTimeLimited.Count);
        }


    }
}
