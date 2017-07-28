using System;
using Master40.Agents.Agents.Internal;
using Master40.DB.Models;

namespace Master40.Agents.Agents.Model
{
    public class WorkItem
    {
        public Guid Id { get; set; }
        public int DueTime { get; set; }
        public double Priority { get; set; }
        public Status Status { get; set; }
        public Agent SourceAgent { get; set; }
        public WorkSchedule WorkSchedule { get; set; }

        public WorkItem()
        {
            Id = Guid.NewGuid();
        }
        
    }
}