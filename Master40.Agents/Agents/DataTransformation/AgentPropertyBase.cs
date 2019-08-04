using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.Agents.Agents.DataTransformation
{
    public abstract class AgentPropertyBase
    {
        protected string PropertyName;

        public string GetPropertyName()
        {
            return PropertyName;
        }
    }
}
