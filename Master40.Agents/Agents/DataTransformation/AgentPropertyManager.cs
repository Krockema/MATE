using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.Agents.Agents.DataTransformation
{
    public static class AgentPropertyManager
    {
        private static Dictionary<string, List<AgentPropertyBase>> PropertySets { get; } = new Dictionary<string, List<AgentPropertyBase>>
        {
            { "ProductionAgent", new List<AgentPropertyBase>
                {
                    new AgentProperty("AgentId", PropertyType.None),
                    new AgentPropertyNode("RequestItem", new List<AgentPropertyBase>
                    {
                        new AgentPropertyNode("Article", new List<AgentPropertyBase>
                        {
                            new AgentProperty("Id", PropertyType.Id),
                            new AgentProperty("UnitId", PropertyType.Id)
                        }),
                        new AgentProperty("OrderId", PropertyType.Id),
                        new AgentProperty("Quantity", PropertyType.None),
                        new AgentProperty("DueTime", PropertyType.None),
                    }),
                    new AgentPropertyNode("WorkItems", new List<AgentPropertyBase>
                    {
                        new AgentProperty("EstimatedStart", PropertyType.Retransform),
                        new AgentProperty("EstimatedEnd", PropertyType.Retransform),
                        //new AgentPropertyNode("WorkSchedule", new List<AgentPropertyBase>
                        //{
                        //    new AgentProperty("Id", PropertyType.Id)
                        //}),
                        new AgentProperty("Id", PropertyType.Id),
                    }),
                }
            },
            { "ContractAgent", new List<AgentPropertyBase>
                {
                    new AgentPropertyNode("requestItem", new List<AgentPropertyBase>
                    {
                        new AgentPropertyNode("Article", new List<AgentPropertyBase>
                        {
                            new AgentProperty("Id", PropertyType.Id)
                        })
                    })
                }

            }
        };

        public static List<AgentPropertyBase> GetPropertiesByAgentName(string agentName)
        {
            return PropertySets.GetValueOrDefault(agentName, new List<AgentPropertyBase>());
        }
    }
}
