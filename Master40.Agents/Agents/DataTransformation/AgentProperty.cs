using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.Agents.Agents.DataTransformation
{
    public class AgentProperty : AgentPropertyBase
    {
        PropertyType Type;

        public AgentProperty(string name, PropertyType type)
        {
            this.PropertyName = name;
            this.Type = type;
        }

        public bool IsId()
        {
            return (Type & PropertyType.Id) == PropertyType.Id;
        }

        public bool IsRetransform()
        {
            return (Type & PropertyType.Retransform) == PropertyType.Retransform;
        }
    }
}
