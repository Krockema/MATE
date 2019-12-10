using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.Preparations;
using Xunit;
using static IJobs;

namespace Master40.XUnitTest.Agents.Resource.Behaviour
{
    public class BucketScope : TestKit
    {
        private JobQueueScopeLimited JobQueueScopeLimited { get; } = new JobQueueScopeLimited(limit: 1000);
        private List<M_ResourceTool> tools { get; set; } = new List<M_ResourceTool>();
        private IActorRef hubAgentActorRef { get; }

        public BucketScope()
        {
            hubAgentActorRef = CreateTestProbe();
        }

        [Fact(Skip = "to implement")]
        public void UpdateAndRequeuePlanedJobs()
        {
            PrepareModel();
            var newJobItem =
                TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 7", jobDuration: 15, dueTime: 45, tool: tools[2]);
            JobQueueScopeLimited.Enqueue(newJobItem);

            var jobsToRequeue = JobQueueScopeLimited.CutTail(0, newJobItem);

            Assert.True(jobsToRequeue.Count.Equals(1));
            Assert.True(jobsToRequeue.Where(x => x.Name.Equals("Sample Operation 6")).ToList().Count.Equals(1));

        }

        [Fact]
        public void GetQueueAbleTime()
        {
            PrepareModel();
            var newJobItem =
                TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 7", jobDuration: 5, dueTime: 140, tool: tools[2]);
            var bucket = MessageFactory.ToBucketScopeItem(newJobItem, hubAgentActorRef, 0);

            var queueableTime = JobQueueScopeLimited.GetQueueAbleTime(job: bucket, currentTime: 0, resourceIsBlockedUntil: 0, processingQueueLength: 0);

            Assert.Equal(expected: 5L, actual: queueableTime.EstimatedStart);
        }

        private void PrepareModel()
        {
            CreateTools();
            CreateBucketItems();
        }

        private void CreateTools()
        {
            tools.Add(new M_ResourceTool() { Id = 0, Name = "BladeBig" });
            tools.Add(new M_ResourceTool() { Id = 1, Name = "BladeMedium" });
            tools.Add(new M_ResourceTool() { Id = 2, Name = "BladeSmall" });
        }


        private void CreateBucketItems()
        {
            var operation1 = TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 1", jobDuration: 10,
                dueTime: 100, tool: tools[0]);
            var operation2 = TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 2", jobDuration: 5,
                dueTime: 150, tool: tools[1]);
            var operation3 = TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 3", jobDuration: 20,
                dueTime: 120, tool: tools[2]);
            var operation4 = TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 4", jobDuration: 5,
                dueTime: 180, tool: tools[0]);
            var operation5 = TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 5", jobDuration: 5,
                dueTime: 180, tool: tools[1]);
            var operation6 = TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 6", jobDuration: 20,
                dueTime: 180, tool: tools[2]);

            var bucket1 = MessageFactory.ToBucketScopeItem(operation1, hubAgentActorRef, 0);
            bucket1.AddOperation(operation4);
            var prioBucket1 = ((IJob)bucket1).Priority(0);
            JobQueueScopeLimited.Enqueue(bucket1);

            var bucket2 = MessageFactory.ToBucketScopeItem(operation2, hubAgentActorRef, 0);
            bucket1.AddOperation(operation5);
            var prioBucket2 = ((IJob)bucket2).Priority(0);
            JobQueueScopeLimited.Enqueue(bucket2);

            var bucket3 = MessageFactory.ToBucketScopeItem(operation3, hubAgentActorRef, 0);
            bucket1.AddOperation(operation3);
            var prioBucket3 = ((IJob)bucket3).Priority(0);
            JobQueueScopeLimited.Enqueue(bucket3);

        }
    }
}
