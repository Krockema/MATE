using System;
using System.Collections.Generic;
using React;
using React.Distribution;

namespace React_Beispielapp
{
    internal class Machine : Process
    {
        private Factory factory;
        private string name;
        private Exponential ex = new Exponential(15);

        internal Machine(Factory factory, string name) : base(factory)
        {   // not required yet
            // this.factory = factory;
            this.name = name;
        }

        protected override IEnumerator<Task> GetProcessSteps()
        {
            Console.WriteLine(Now + " Start with Workschedule on " + name);
            var d = Convert.ToInt32(ex.NextDouble() * 10) + 14;
            yield return Delay(d);
            Console.WriteLine(Now + " Finished with Workschedule after: " + d + " min");
            yield break;
        }
    }
}