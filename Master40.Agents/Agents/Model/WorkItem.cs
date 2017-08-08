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
        public int EsitamtedEnd { get; set; }
        public double Priority { get; set; }
        public Status Status { get; set; }
        public Guid MachineAgentId { get; set; }
        public Agent ProductionAgent { get; set; }
        public Agent ComunicationAgent { get; set; }
        public WorkSchedule WorkSchedule { get; set; }
        public List<Proposal> Proposals { get; set; }
        public WorkItem()
        {
            Id = Guid.NewGuid();
            Proposals = new List<Proposal>();

        }
    }


    
}