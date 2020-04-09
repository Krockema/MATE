using Akka.Actor;
using Akka.TestKit.Xunit;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.ResourceAgent.Types.TimeConstraintQueue;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.Online.Preparations;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static FJobConfirmations;
using static IJobs;

namespace Master40.XUnitTest.Online.Agents.Resource.Behaviour
{
    public class BucketScope : TestKit
    {
        private TimeConstraintQueue JobQueueScopeLimited { get; } = new TimeConstraintQueue(limit: 1000);
        private List<M_ResourceCapability> tools { get; set; } = new List<M_ResourceCapability>();
        private IActorRef hubAgentActorRef { get; }

        public BucketScope()
        {
            hubAgentActorRef = CreateTestProbe();
        }

        [Fact(Skip = "to implement")]
        public void UpdateAndRequeuePlanedJobs()
        {
            PrepareModel();
            var newJobItem = new FJobConfirmation(TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 7", jobDuration: 15,
                    dueTime: 45, capability: tools[2]), 20, 15,
                null);
            JobQueueScopeLimited.Enqueue(newJobItem);

            var jobsToRequeue = JobQueueScopeLimited.CutTail(0, newJobItem);

            Assert.True(jobsToRequeue.Count.Equals(1));
            Assert.True(jobsToRequeue.Where(x => x.Job.Name.Equals("Sample Operation 6")).ToList().Count.Equals(1));

        }

        [Fact]
        public void GetQueueAbleTime()
        {
            PrepareModel();
            
            var newJobItem =
                TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 7", jobDuration: 5, dueTime: 140, capability: tools[2]);
            
            var bucket = newJobItem.ToBucketScopeItem(hubAgentActorRef, 0, 480);
            var jobProposalRequest = new FRequestProposalForCapabilityProviders.FRequestProposalForCapabilityProvider(bucket, newJobItem.SetupKey);
            var queueableTime = JobQueueScopeLimited.GetQueueAbleTime(jobProposalRequest, currentTime: 0, cpm: null).First();

            Assert.Equal(expected: 125L, actual: queueableTime.EstimatedStart);
        }

        private void PrepareModel()
        {
            CreateTools();
            CreateBucketItems();
        }

        private void CreateTools()
        {
            tools.Add(new M_ResourceCapability() { Id = 0, Name = "BladeBig" });
            tools.Add(new M_ResourceCapability() { Id = 1, Name = "BladeMedium" });
            tools.Add(new M_ResourceCapability() { Id = 2, Name = "BladeSmall" });
        }


        private void CreateBucketItems()
        {
            var operation1 = TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 1", jobDuration: 10,
                dueTime: 100, capability: tools[0]);
            var operation2 = TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 2", jobDuration: 5,
                dueTime: 150, capability: tools[1]);
            var operation3 = TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 3", jobDuration: 20,
                dueTime: 120, capability: tools[2]);
            var operation4 = TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 4", jobDuration: 5,
                dueTime: 180, capability: tools[0]);
            var operation5 = TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 5", jobDuration: 5,
                dueTime: 180, capability: tools[1]);
            var operation6 = TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 6", jobDuration: 20,
                dueTime: 180, capability: tools[2]);

            var bucket1 = operation1.ToBucketScopeItem(hubAgentActorRef, 0, 480);
            bucket1.AddOperation(operation4);
            var prioBucket1 = ((IJob)bucket1).Priority(0);
            bucket1.StartConditions.ArticlesProvided = true;
            bucket1.StartConditions.PreCondition = true;

            var bucketConfirmation1 = new FJobConfirmation(bucket1, 20, 20,
                 null);
            JobQueueScopeLimited.Enqueue(bucketConfirmation1);

            var bucket2 = operation2.ToBucketScopeItem(hubAgentActorRef, 0, 480);
            bucket2.AddOperation(operation5);
            var prioBucket2 = ((IJob)bucket2).Priority(0);
            bucket2.StartConditions.ArticlesProvided = true;
            bucket2.StartConditions.PreCondition = true;
            var bucketConfirmation2 = new FJobConfirmation(bucket2, 20, 10,
                null);
            JobQueueScopeLimited.Enqueue(bucketConfirmation2);

            var bucket3 = operation3.ToBucketScopeItem(hubAgentActorRef, 0, 480);
            bucket3.AddOperation(operation3);
            var prioBucket3 = ((IJob)bucket3).Priority(0);
            bucket3.StartConditions.ArticlesProvided = true;
            bucket3.StartConditions.PreCondition = true;
            var bucketConfirmation3 = new FJobConfirmation(bucket3, 20, 10,
                null);
            JobQueueScopeLimited.Enqueue(bucketConfirmation3);

        }
    }
}
