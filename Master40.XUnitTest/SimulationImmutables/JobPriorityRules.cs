using Akka.TestKit.Xunit;
using Master40.DB.DataModel;
using Master40.SimulationCore.Helper;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using static IJobs;

namespace Master40.XUnitTest.SimulationImmutables
{
    public class JobPriorityRules : TestKit
    {

        [Fact]
        public async Task FArticleRule()
        {
            await Task.Run(action: () =>
            {
                var probe = this.CreateTestProbe();

                var wi = MessageFactory.ToOperationItem(new M_Operation() { Duration = 5 }, 15, productionAgent: probe, firstOperation: false, 0);

                var w1 = ((IJob)wi).Priority(0);
                Debug.WriteLine(value: w1);

                Assert.Equal(expected: 10, actual: w1);
            });
        }

        [Fact]
        public async Task FBucketPriority()
        {
            await Task.Run(action: () =>
            {
                var time = 0L;
                var probe = this.CreateTestProbe();

                var w1 = MessageFactory.ToOperationItem(new M_Operation() { Duration = 10 , ResourceSkill = new M_ResourceSkill() { Name = "Saw blade big"} }, 50, productionAgent: probe, firstOperation: false ,  0);
                var w2 = MessageFactory.ToOperationItem(new M_Operation() { Duration = 5 , ResourceSkill = new M_ResourceSkill() { Name = "Saw blade big" } }, 20, productionAgent: probe, firstOperation: false, 0);
                var w3 = MessageFactory.ToOperationItem(new M_Operation() { Duration = 15 , ResourceSkill = new M_ResourceSkill() { Name = "Saw blade big" } }, 100, productionAgent: probe, firstOperation: false, 0);

                var bucket1 = MessageFactory.ToBucketItem(operation: w1, time: time);
                var prio1 = ((IJob)bucket1).Priority(time);
                Assert.Equal(expected: prio1, actual: (double)40);

                bucket1 = bucket1.AddOperation(op: w2);
                var prio2 = ((IJob)bucket1).Priority(time);
                Assert.Equal(expected: prio2, actual: (double)15);

                bucket1 = bucket1.AddOperation(op: w3);
                var prio3 = ((IJob)bucket1).Priority(time);
                Assert.Equal(expected: prio3, actual: (double)15);

                bucket1 = bucket1.RemoveOperation(op: w2);
                var prio4 = ((IJob)bucket1).Priority(time);
                Assert.Equal(expected: prio4, actual: (double)40);
            });
        }

    }
}
