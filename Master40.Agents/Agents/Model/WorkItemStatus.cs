using System;
using Master40.Agents.Agents.Internal;

namespace Master40.Agents.Agents.Model
{
    public class WorkItemStatus
    {
        public Guid WorkItemId { get; set; }
        public Status Status { get; set; }
        public double CurrentPriority { get; set; }
    }
}