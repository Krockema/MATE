using System;
using System.Collections.Generic;
using Master40.Agents.Agents.Internal;
using Master40.DB.Data.Helper;
using Master40.DB.Interfaces;
using Master40.DB.Models;

namespace Master40.Agents.Agents.Model
{
    public class WorkItem
    {
        public Guid Id { get; set; }
        public int DueTime { get; set; }
        public int EstimatedStart { get; set; }
        public int EstimatedEnd { get; set; }
        public bool MaterialsProvided { get; set; }
        //public double Priority { get; set; }
        private double _priority;
        public Func<long, double> PriorityRule { get; set; }
        public Status Status { get; set; }
        public bool WasSetReady { get; set; }
        public Guid MachineAgentId { get; set; }
        public Agent ProductionAgent { get; set; }
        public Agent ComunicationAgent { get; set; }
        public WorkSchedule WorkSchedule { get; set; }
        public List<Proposal> Proposals { get; set; }
        public WorkItem()
        {
            Id = Guid.NewGuid();
            Proposals = new List<Proposal>();
            PriorityRule = currentTime => DueTime - WorkSchedule.Duration - currentTime; 
        }
        public double Priority() { return _priority; }
        public double Priority(long time) {
            _priority = PriorityRule(time);
            return _priority; }
    }
}