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

        [Fact]
        void FindAndAddBucket()
        {
            BucketManager bucketManager = CreateTestSetForBuckets();

            //Add operation to Bucket(0)
            M_ResourceTool tool1 = new M_ResourceTool() { Name = "SawBig" };
            var operation1 = TypeFactory.CreateDummyJobItem(jobName: "Job1", jobDuration: 25, averageTransitionDuration: 10, dueTime: 50, tool: tool1);
            var bucket = bucketManager.FindAndAddBucket(operation1, ActorRefs.Nobody, currentTime:0);
            Assert.Equal(3, bucket.Operations.Count);

            //Adds operation to Bucket(1) because first one is over capacity
            var operation2 = TypeFactory.CreateDummyJobItem(jobName: "Job1", jobDuration: 70, averageTransitionDuration: 10, dueTime: 50, tool: tool1);
            bucket = bucketManager.FindAndAddBucket(operation1, ActorRefs.Nobody, currentTime: 0);
            Assert.Equal(2, bucket.Operations.Count);

            //Create new bucket because ForwardTime is earlier / see currentTime
            var operation3 = TypeFactory.CreateDummyJobItem(jobName: "Job1", currentTime: -10, jobDuration: 70, averageTransitionDuration: 10, dueTime: 50, tool: tool1);
            bucket = bucketManager.FindAndAddBucket(operation3, ActorRefs.Nobody, currentTime: 0);
            Assert.Null(bucket);

        }


        [Fact]
        void ModifyBucket()
        {


        }

        [Fact]
        void CreateBucket()
        {
            BucketManager bucketManager = new BucketManager();
            M_ResourceTool tool = new M_ResourceTool(){Name = "SawBig"};
            var operationJob = TypeFactory.CreateDummyJobItem(jobName: "Job1", jobDuration: 35, averageTransitionDuration: 10, tool: tool);

            var bucket = bucketManager.CreateBucket(operationJob, hubAgent: ActorRefs.Nobody, currentTime: 0);

            Assert.Equal(expected: 1, actual: bucket.Operations.Count);

        }

        [Fact]
        void BackwardAndForwardTestForBucket()
        {
            BucketManager bucketManager = new BucketManager();
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
            BucketManager bucketManager = new BucketManager();
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

        /// <summary>
        /// Provides a TestSet of Buckets for UnitTests
        /// IF CHANGES ARE MADE HERE CHECK ALL REFERENCED UNIT TESTS
        /// </summary>
        BucketManager CreateTestSetForBuckets()
        {
            BucketManager bucketManager = new BucketManager();
            M_ResourceTool tool1 = new M_ResourceTool() { Name = "SawBig" };
            M_ResourceTool tool2 = new M_ResourceTool() { Name = "SawSmall" };

            //Bucket1 Saw Big
            var operation1 = TypeFactory.CreateDummyJobItem(jobName: "Job1", jobDuration: 25, averageTransitionDuration: 10, dueTime: 150, tool: tool1);
            var operation2 = TypeFactory.CreateDummyJobItem(jobName: "Job2", jobDuration: 25, averageTransitionDuration: 10, dueTime: 150, tool: tool1);
            var bucket = bucketManager.CreateBucket(operation1, hubAgent: ActorRefs.Nobody, currentTime: 0);
            bucket = bucket.AddOperation(operation2);
            bucketManager.Replace(bucket);

            //Bucket2 Saw Big
            var operation3 = TypeFactory.CreateDummyJobItem(jobName: "Job3", jobDuration: 15, averageTransitionDuration: 10, dueTime: 150, tool: tool1);
            bucketManager.CreateBucket(operation3, hubAgent: ActorRefs.Nobody, currentTime: 0);
            
            //Bucket3 Saw Small
            var operation4 = TypeFactory.CreateDummyJobItem(jobName: "Job4", jobDuration: 15, averageTransitionDuration: 10, dueTime: 150, tool: tool2);
            bucketManager.CreateBucket(operation4, hubAgent: ActorRefs.Nobody, currentTime: 0);

            return bucketManager; 


        }

    }
}
