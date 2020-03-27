using System.Collections.Generic;
using System.Linq;
using Akka.TestKit.Xunit;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.XUnitTest.Online.Preparations;
using Xunit;

namespace Master40.XUnitTest.Online.Agents.Contract.Behaviour
{
    public class DefaultSetupStack : TestKit
    {
        private JobQueueTimeLimited jobQueueTimeLimited { get; }= new JobQueueTimeLimited(limit: 45);
        private List<M_ResourceCapability> tools { get; set; } = new List<M_ResourceCapability>();

        [Fact]
        public void UpdateAndRequeuePlanedJobs()
        {
            PrepareModel();
            var newJobItem =
                TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 7", jobDuration: 15, dueTime: 45, capability: tools[2]);
            jobQueueTimeLimited.Enqueue(newJobItem);

            var jobsToRequeue = jobQueueTimeLimited.CutTailByStack(0, newJobItem);

            Assert.True(jobsToRequeue.Count.Equals(1));
            Assert.True(jobsToRequeue.Where(x => x.Name.Equals("Sample Operation 6")).ToList().Count.Equals(1));

        }

        [Fact]
        public void GetQueueAbleTimeByStack()
        {
            PrepareModel();
            var newJobItem =
                TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 7", jobDuration: 5, dueTime: 35, capability: tools[2]);

            var queableTime= jobQueueTimeLimited.GetQueueAbleTimeByStack(job: newJobItem, currentTime: 0,resourceIsBlockedUntil:0, processingQueueLength:0);

            Assert.Equal(expected: 25L, actual: queableTime.EstimatedStart);
        }
        
        private void PrepareModel()
        {
            CreateTools();
            CreateJobItems();
        }

        private void CreateTools()
        {
            tools.Add(new M_ResourceCapability() { Id = 0, Name = "BladeBig" });
            tools.Add(new M_ResourceCapability() { Id = 1, Name = "BladeMedium" });
            tools.Add(new M_ResourceCapability() { Id = 2, Name = "BladeSmall" });
        }



        private void CreateJobItems()
        {
            jobQueueTimeLimited.Enqueue(item: TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 1", jobDuration: 5,
                dueTime: 10, capability: tools[0]));
            jobQueueTimeLimited.Enqueue(item: TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 2", jobDuration: 5,
                dueTime: 20, capability: tools[1]));
            jobQueueTimeLimited.Enqueue(item: TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 3", jobDuration: 5,
                dueTime: 30, capability: tools[2]));
            jobQueueTimeLimited.Enqueue(item: TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 4", jobDuration: 5,
                dueTime: 40, capability: tools[0]));
            jobQueueTimeLimited.Enqueue(item: TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 5", jobDuration: 5,
                dueTime: 50, capability: tools[1]));
            jobQueueTimeLimited.Enqueue(item: TypeFactory.CreateDummyJobItem(jobName: "Sample Operation 6", jobDuration: 20,
                dueTime: 80, capability: tools[2]));
        }


    }
}
