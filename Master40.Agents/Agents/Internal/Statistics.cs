using System;
using System.Collections.Generic;
using System.Linq;
using Master40.Agents.Agents.Model;
using Master40.DB.Enums;
using Master40.DB.Models;

namespace Master40.Agents.Agents.Internal
{
    public static class Statistics
    {
        public static void CreateSimulationWorkSchedule(WorkItem ws, string orderId)
        {
            var sws = new SimulationWorkschedule
            {
                WorkScheduleId = ws.Id.ToString(),
                Article = ws.WorkSchedule.Article.Name,
                WorkScheduleName = ws.WorkSchedule.Name,
                DueTime = ws.DueTime,
                EstimatedEnd = ws.EstimatedEnd,
                SimulationId = -1,
                OrderId = orderId,
                HierarchyNumber = ws.WorkSchedule.HierarchyNumber,
                ProductionOrderId = ws.ProductionAgent.AgentId.ToString()
            };
            AgentSimulation.SimulationWorkschedules.Add(sws);
        }


        public static void UpdateSimulationWorkSchedule(string workScheduleId, int start, int duration, Machine machine)
        {
            var edit = AgentSimulation.SimulationWorkschedules.FirstOrDefault(x => x.WorkScheduleId.Equals(workScheduleId));
            edit.Start = start;
            edit.End = start + duration + 1; // to have Time Points instead of Time Periods
            edit.Machine = machine.Name;
        }

        public static void UpdateSimulationId(int simulationId)
        {
            var simItems = AgentSimulation.SimulationWorkschedules
                .Where(x => x.SimulationId == -1).ToList();
            foreach (var item in simItems)
            {
                item.SimulationId = simulationId;
                item.SimulationType = SimulationType.Decentral.ToString();
            }
        }

        internal static void UpdateSimulationWorkSchedule(List<Guid> ProductionAgents, Agent RequesterAgent, int orderId)
        {
            foreach (var AgentId in ProductionAgents)
            {
                var items = AgentSimulation.SimulationWorkschedules.Where(x => x.ProductionOrderId == AgentId.ToString());
                foreach (var item in items)
                {
                    item.Parent = RequesterAgent.Name;
                    item.ParentId = RequesterAgent.AgentId.ToString();
                   // item.OrderId = orderId;
                }
            }

        }
    }
}