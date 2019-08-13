using Akka.TestKit.Xunit;
using Master40.DB.DataModel;
using Master40.SimulationCore.Helper;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using static IJobs;

namespace Master40.XUnitTest.SimulationImmutables
{
    public class Immutables : TestKit
    {

        [Fact]
        public async Task PriorityRule()
        {
            await Task.Run(() =>
            {
                var probe = this.CreateTestProbe();

                var wi = MessageFactory.ToOperationItem(new M_Operation() { Duration = 5 }, 15, productionAgent: probe, lastLeaf: false, 0);

                var w1 = ((IJob)wi).Priority(0);
                Debug.WriteLine(w1);

                Assert.Equal(10, w1);
            });
        }

        [Fact]
        public async Task PriorityRuleBucket()
        {
            await Task.Run(() =>
            {
                var time = 0L;
                var probe = this.CreateTestProbe();

                var w1 = MessageFactory.ToOperationItem(new M_Operation() { Duration = 10 }, 50, productionAgent: probe, lastLeaf: false, 0);
                var w2 = MessageFactory.ToOperationItem(new M_Operation() { Duration = 5 }, 20, productionAgent: probe, lastLeaf: false, 0);
                var w3 = MessageFactory.ToOperationItem(new M_Operation() { Duration = 15 }, 100, productionAgent: probe, lastLeaf: false, 0);

                var bucket1 = MessageFactory.ToBucketItem(w1, time);
                var prio1 = ((IJob)bucket1).Priority(time);
                Assert.Equal(prio1, (double)40);

                bucket1 = bucket1.AddOperation(w2);
                var prio2 = ((IJob)bucket1).Priority(time);
                Assert.Equal(prio2, (double)15);

                bucket1 = bucket1.AddOperation(w3);
                var prio3 = ((IJob)bucket1).Priority(time);
                Assert.Equal(prio3, (double)15);

                bucket1 = bucket1.RemoveOperation(w2);
                var prio4 = ((IJob)bucket1).Priority(time);
                Assert.Equal(prio4, (double)40);
            });
        }

    }
}
