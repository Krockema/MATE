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
        
        public List<AgentPropertyBase> GetProperties()
        {
            return Properties;
        }
    }
}
