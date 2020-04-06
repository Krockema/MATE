using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Master40.Tools.TimeScopedQueue
{
    public class TimeScope // evtl. FTimeScope
    {
        public long Start { get; set; }
        public long Duration { get; set; }
        public long End => Start + Duration;
        public Object PlanedItem { get; set; }
    }
}
