using Master40.SimulationCore.Agents.ResourceAgent.Types.TimeConstraintQueue;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static FQueueingPositions;

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
            _queue.Add(1, new FJobConfirmations.FJobConfirmation(null, new FQueueingPosition(true, true, 1, 3, 2), 2, null));
            _queue.Add(6, new FJobConfirmations.FJobConfirmation(null, new FQueueingPosition(true, true, 6, 8, 2), 2, null));
            _queue.Add(9, new FJobConfirmations.FJobConfirmation(null, new FQueueingPosition(true, true, 9, 10, 1), 1, null));
            _queue.Add(10, new FJobConfirmations.FJobConfirmation(null, new FQueueingPosition(true, true, 10, 12, 2), 2, null));
        }

        [Fact]
        public void QueueIsValid()
        {
            var enumerator = _queue.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                Assert.True(false);
                return;
            }

            var (key, value) = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var end = key + value.Duration;
                var startNext = enumerator.Current;

                Assert.True(end <= startNext.Key);
                (key, value) = enumerator.Current;
            }
            enumerator.Dispose();
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
                    var endPre = current.Key + current.Value.QueueingPosition.Start;
                    var startPost = enumerator.Current.Key;

                    if (endPre <= startPost - tDuration)
                    {
                        slotFound = validSlots.TryAdd(endPre, startPost - endPre);
                        break;
                    }
                    current = enumerator.Current;
                }

                if (!slotFound)
                    validSlots.Add(current.Key + current.Value.QueueingPosition.Start, long.MaxValue);
            }
            enumerator.Dispose();

            Assert.Equal(tExpectedLength, validSlots.Count);

            validSlots.ToList().ForEach(x => _output.WriteLine($"start: {x.Key}, slotSize {x.Value} "));
        }
    }
}
