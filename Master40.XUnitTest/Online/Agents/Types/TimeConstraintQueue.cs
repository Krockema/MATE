using Master40.SimulationCore.Agents.ResourceAgent.Types.TimeConstraintQueue;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Master40.XUnitTest.Online.Agents.Types
{
    public class TimeConstraintQueueTest
    {
        private readonly ITestOutputHelper _output;
        // Conflicting Time Sets to Test.
        //  
        // Existing Time Set          |------------------|
        // Overlapping Start       |----------|
        // Overlapping End                         |---------|
        // Subset                         |-----------|
        // Superset              |-----------------------------|


        private TimeConstraintQueue _queue;
        public TimeConstraintQueueTest(ITestOutputHelper output)
        {
            _queue = new TimeConstraintQueue(100);
            _output = output;
            // T0 1 2 3 4 5 6 7 8 9 10 11 12 13 
            // I0 1 1 - - - 2 2 - 3  4  4  -  -   
            _queue.Add(1, new FJobConfirmations.FJobConfirmation(null, 1, 2, null));
            _queue.Add(6, new FJobConfirmations.FJobConfirmation(null, 6, 2, null));
            _queue.Add(9, new FJobConfirmations.FJobConfirmation(null, 9, 1, null));
            _queue.Add(10, new FJobConfirmations.FJobConfirmation(null, 10, 2, null));
        }

        [Fact]
        public void QueueIsValid()
        {
            var nextItem = _queue.GetEnumerator();
            var (key, value) = nextItem.Current;

            while (nextItem.MoveNext())
            {
                var end = key + value.Duration;
                var startNext = nextItem.Current;

                Assert.True(end <= startNext.Key);
                (key, value) = nextItem.Current;
            }
            nextItem.Dispose();
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(4, 1)]
        public void FindFirstValidSlot(long tDuration, int tExpectedLength)
        {
            var validSlots = new SortedList<long, long>();
            var enumerator = _queue.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                validSlots.Add(0, tDuration);
            } else { 
                var current = enumerator.Current;
                var slotFound = false;
                while (enumerator.MoveNext())
                {
                    var endPre = current.Key + current.Value.Schedule;
                    var startPost = enumerator.Current.Key;

                    if (endPre <= startPost - tDuration)
                    {
                        slotFound = validSlots.TryAdd(endPre, startPost - endPre);
                        break;
                    }
                    current = enumerator.Current;
                }

                if (!slotFound)
                    validSlots.Add(current.Key + current.Value.Schedule, long.MaxValue);
            }
            enumerator.Dispose();

            Assert.Equal(tExpectedLength, validSlots.Count);

            validSlots.ToList().ForEach(x => _output.WriteLine($"start: {x.Key}, slotSize {x.Value} "));
        }
    }
}
