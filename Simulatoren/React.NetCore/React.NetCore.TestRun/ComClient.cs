using System.Collections.Generic;
using System.Linq;
using React;

namespace React_Beispielapp
{
    public class ComClient : Process
    {
        public List<WorkSchedule> wsl { get; set; }
        public List<Resource> rl { get; set; }
        // public string Name { get; set; }
        internal ComClient(Factory factory, string name) : base(factory)
        {   // not required yet
            // this.factory = factory;
            this.Name = name;
        }

        protected override IEnumerator<Task> GetProcessSteps()
        {
            foreach (WorkSchedule ws in wsl.Where(x => x.ItemState != ItemState.Queued &&
                                                       x.ItemState != ItemState.Finished))
            {
                var schilds = wsl.FindAll(x => x.WorkScheduleId == ws.Id && x.Id != ws.Id);
                var getRedy = schilds.Where(x => x.ItemState == ItemState.Created
                                                 || x.ItemState == ItemState.Ready
                                                 || x.ItemState == ItemState.Queued);

                if (getRedy.Any()) continue;

                ws.ItemState = ItemState.Queued;
                ws.Activate(null, 0L, rl.FirstOrDefault());
            }
            yield break;
        }

    }
}