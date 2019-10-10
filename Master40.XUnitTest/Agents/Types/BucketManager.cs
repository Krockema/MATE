using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.XUnitTest.Preparations;
using Xunit;

namespace Master40.XUnitTest.Agents.Types
{
    public class BucketManagers : TestKit
    {
        private BucketManager bucketManager = new BucketManager();


        [Fact]
        void FindOrCreateBucket()
        {
            
        }

        [Fact]
        void ModifyBucket()
        {

        }

        [Fact]
        void CreateBucket()
        {
            M_ResourceTool tool = new M_ResourceTool(){Name = "SawBig"};
            var operationJob = TypeFactory.CreateDummyJobItem(jobName: "Job1", jobDuration: 5, averageTransitionDuration: 10, tool: tool);

            var bucket = bucketManager.CreateBucket(operationJob, hubAgent: ActorRefs.Nobody, currentTime: 0);

            Assert.Equal(expected: 1, actual: bucket.Operations.Count);

        }

        [Fact]
        void BackwardAndForwardTestForBucket()
        {
            M_ResourceTool tool = new M_ResourceTool() { Name = "SawBig" };
            var operationJob = TypeFactory.CreateDummyJobItem(jobName: "Job1", jobDuration: 5, averageTransitionDuration: 10, tool: tool);

            var bucket = bucketManager.CreateBucket(operationJob, hubAgent: ActorRefs.Nobody, currentTime: 0);

            //Forward
            Assert.Equal(expected: 0, actual: bucket.ForwardStart);
            Assert.Equal(expected: 15, actual: bucket.ForwardEnd);

            //Backward
            Assert.Equal(expected: 35, actual: bucket.BackwardStart);
            Assert.Equal(expected: 50, actual: bucket.BackwardEnd);
        }

        [Fact]
        void BackwardAndForwardTestForModifiedBucket()
        {
            M_ResourceTool tool = new M_ResourceTool() { Name = "SawBig" };
            var operationJob = TypeFactory.CreateDummyJobItem(jobName: "Job1", jobDuration: 5, averageTransitionDuration: 10, dueTime: 50, tool: tool);

            var bucket = bucketManager.CreateBucket(operationJob, hubAgent: ActorRefs.Nobody, currentTime: 0);

            //Forward
            Assert.Equal(expected: 0, actual: bucket.ForwardStart);
            Assert.Equal(expected: 15, actual: bucket.ForwardEnd);

            //Backward
            Assert.Equal(expected: 35, actual: bucket.BackwardStart);
            Assert.Equal(expected: 50, actual: bucket.BackwardEnd);

            var incomingOperationJob = TypeFactory.CreateDummyJobItem(jobName: "Job1", jobDuration: 5, averageTransitionDuration: 10, dueTime: 70, tool: tool);
            bucket.AddOperation(incomingOperationJob);

            //Forward
            Assert.Equal(expected: 0, actual: bucket.ForwardStart);
            Assert.Equal(expected: 15, actual: bucket.ForwardEnd);

            //Backward
            Assert.Equal(expected: 35, actual: bucket.BackwardStart);
            Assert.Equal(expected: 50, actual: bucket.BackwardEnd);

        }

    }
}
