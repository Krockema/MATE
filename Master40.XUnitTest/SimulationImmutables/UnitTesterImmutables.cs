using Akka.Actor;
using Akka.TestKit.Xunit;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

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

                var wi = MessageFactory.ToWorkItem(new M_Operation() { Duration = 5 }, 15, ElementStatus.Created, probe, 0);

                var w1 = wi.Priority(10);
                var w2 = wi.GetPriority(10);
                var w3 = wi.ItemPriority;
                Debug.WriteLine(w1);
                Debug.WriteLine(w2);

                Assert.Equal(w1, w2);
                Assert.Equal(w1, w3);
            });
        }

        [Fact]
        public async Task PriorityRuleBucket()
        {
            await Task.Run(() =>
            {

                var time = 0;
                var probe = this.CreateTestProbe();

                var w1 = MessageFactory.ToWorkItem(new M_Operation() { Duration = 10 }, 50, ElementStatus.Created, probe, 0);
                var w2 = MessageFactory.ToWorkItem(new M_Operation() { Duration = 5 }, 20, ElementStatus.Ready, probe, 0);
                var w3 = MessageFactory.ToWorkItem(new M_Operation() { Duration = 15 }, 100, ElementStatus.Created, probe, 0);


                var bucket1 = MessageFactory.ToBucketItem(w1, time);
                bucket1.Priority(time);

                bucket1 = bucket1.AddOperation(w2);
                bucket1.Priority(time);
                Assert.Equal(bucket1.ItemPriority, (double)15);
                Assert.Equal(bucket1.DueTime, (double)20);

                bucket1 = bucket1.AddOperation(w3);
                bucket1.Priority(time);
                Assert.Equal(bucket1.ItemPriority, (double)15);
                Assert.Equal(bucket1.DueTime, (double)20);

                bucket1 = bucket1.RemoveOperation(w2);
                bucket1.Priority(time);
                Assert.Equal(bucket1.ItemPriority, (double)40);
                Assert.Equal(bucket1.DueTime, (double)50);
            });
        }

    }
    }
