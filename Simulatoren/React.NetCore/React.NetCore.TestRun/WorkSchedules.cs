using System;
using System.Collections.Generic;
using React;

namespace React_Beispielapp
{
    public class WorkSchedule : Process
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int WorkScheduleId { get; set; }
        public ItemState ItemState { get; set; }
        public virtual List<WorkSchedule> WorkSchedules { get; set; }


        public WorkSchedule(Factory factory) : base(factory)
        {
            ItemState = ItemState.Ready;
        }

        protected override IEnumerator<Task> GetProcessSteps()
        {
            Resource machines = (Resource) ActivationData;
            yield return machines.Acquire(this);
            
            System.Diagnostics.Debug.Assert(machines == Activator);
            System.Diagnostics.Debug.Assert(ActivationData != null);
            Machine machine = ActivationData as Machine;

            WaitOnTask(machine);
            yield return Suspend();

            this.ItemState = ItemState.Finished;
            Console.WriteLine(Now + " " + this.Name + "is done!");

            yield return machines.Release(this);

        }

    }
}