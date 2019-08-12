using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.Agents.Agents.DataTransformation
{
    public class AgentPropertyNode : AgentPropertyBase
    {
        List<AgentPropertyBase> Properties;

        public AgentPropertyNode(string name, List<AgentPropertyBase> props)
        {
            this.PropertyName = name;
            this.Properties = props;
        }

        public override bool IsNode()
        {
            return true;
        }

        public List<AgentPropertyBase> GetProperties()
        {
            return Properties;
        }

        public List<AgentProperty> GetDirectProperties(PropertyType type = PropertyType.None)
        {
            List<AgentProperty> props = new List<AgentProperty>();
            foreach(AgentPropertyBase prop in Properties)
            {
                if(!prop.IsNode())
                {
                    AgentProperty agentProp = (AgentProperty)prop;
                    if ((agentProp.GetPropertyType() & type) == type)
                    {
                        props.Add(agentProp);
                    }
                }
            }
            return props;
        }

        public List<AgentPropertyNode> GetPropertyNodes()
        {
            List<AgentPropertyNode> propNodes = new List<AgentPropertyNode>();
            foreach(AgentPropertyBase prop in Properties)
            {
                if(prop.IsNode())
                {
                    propNodes.Add((AgentPropertyNode)prop);
                }
            }
            return propNodes;
        }
    }
}
