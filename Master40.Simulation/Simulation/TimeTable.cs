using System.Collections.Generic;
using System.Linq;
using Master40.DB.DB.Models;

namespace Master40.Simulation.Simulation
{
    public class TimeTable<T> where T : ISimulationItem
    {
        public TimeTable(int recalculateTimer = -1)
        {
            Timer = 0;
            RecalculateTimer = recalculateTimer;
            Items = new List<T>();
        }

        public List<T> Items { get; set; }

        public int Timer { get; set; }
        public int RecalculateTimer { get; set; }

        public TimeTable<ISimulationItem> ProcessTimeline(TimeTable<ISimulationItem> timeTable)
        {
            if (!timeTable.Items.Any()) return timeTable;
            var start = RecalculateTimer + 1;
            var startItems = timeTable.Items.Where(a => a.SimulationState == SimulationState.Waiting).ToList();
            if (startItems.Any()) start = startItems.Min(a => a.Start);
            var end = RecalculateTimer + 1;
            var endItems = timeTable.Items.Where(a => a.SimulationState == SimulationState.InProgress).ToList();
            if (endItems.Any()) end = endItems.Min(a => a.End);
            // Timewarp - set Start Time
            if (timeTable.RecalculateTimer < start && timeTable.RecalculateTimer < end)
            {
                timeTable.Timer = timeTable.RecalculateTimer;
                return timeTable;
            }
            else
                timeTable.Timer = start < end ? start : end;
            
            foreach (var item in (from tT in timeTable.Items where tT.Start == timeTable.Timer || tT.End == timeTable.Timer select tT).ToList())
            {
                if (item.Start == timeTable.Timer)
                    AddToInProgress(item);
                else
                    HandleFinishedItems(item);
                //check if changing the item is enough and the list automatically updates
                if (!timeTable.Items.Contains(item))
                    throw null;
            }

            // if Progress is empty Stop.
            return timeTable;
        }

        private void HandleFinishedItems(ISimulationItem item)
        {
            item.SimulationState = SimulationState.Finished;
            item.DoAtEnd();
        }

        private void AddToInProgress(ISimulationItem item)
        {
            item.SimulationState = SimulationState.InProgress;
            item.DoAtStart();
        }

    }
}
