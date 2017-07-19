using System;
using System.Collections.Generic;
using React;
using React.Distribution;

namespace React_Beispielapp
{
    public class Factory : Simulation
    {
        private const long simLength = 24 * 60;

        public IEnumerator<Task> Generator(Process p, object data)
        {
            
            Console.WriteLine("Factory Start...");
            Resource machineGroup = CreateMachineGroup();

            Normal n = new Normal(10, 1);
            List<WorkSchedule> wsl = CreateInitialWorkSchedules();
            ComClient cc = new ComClient(this, "ComClient");
            cc.wsl = wsl;
            cc.rl = new List<Resource> { machineGroup };
            do
            {
                cc.Activate(null, 0L);
                yield return p.Delay(1);
            } while (Now < simLength);
            /*
            do
            {
                long d;
                do
                {
                    d = (long)n.NextDouble();
                } while (d <= 0L);

                yield return p.Delay(d);

                
                

            } while (Now < simLength);
            */
            Console.WriteLine("Day Over");

            if (machineGroup.BlockCount > 0)
            {
                Console.WriteLine("There is still work to do...");
            }
        }

        private List<WorkSchedule> CreateInitialWorkSchedules()
        {
            List<WorkSchedule> wsl = new List<WorkSchedule>();
            for (int i = 0; i < 5; i++)
            {
                var r = new Random();
                
                var pa = r.Next(0, wsl.Count);
                var s = new WorkSchedule(this)
                {
                    Id = i,

                    WorkScheduleId = pa,
                    Name = "Item: " + i + " Required by: " + pa + "!"
                };
                wsl.Add(s);
                wsl[s.WorkScheduleId].ItemState = ItemState.Created;
            }

            return wsl;
        }

        private Resource CreateMachineGroup()
        {
            return Resource.Create(new[] {
                  new Machine(this, "M_ 1 _!")
                , new Machine(this, "M_ 2 _!")
            });
        }
    }

}
