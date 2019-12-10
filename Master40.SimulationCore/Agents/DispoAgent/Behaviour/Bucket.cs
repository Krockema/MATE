using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master40.SimulationCore.Agents.DispoAgent.Behaviour
{
    public class Bucket : Default
    {
        internal Bucket() : base(simulationType: SimulationType.Bucket) { }

        public override bool Action(object message)
        {
            switch (message)
            {
                case BasicInstruction.ResponseFromDirectory r: base.ResponseFromDirectory(hubInfo: r.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }


    }
}
