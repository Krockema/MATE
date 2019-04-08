using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.Models;

namespace Master40.DB.DataTransformation
{
    public class DestinationRuleGroup
    {
        public List<Mapping> Rules { get; } = new List<Mapping>();

        public DestinationRuleGroup()
        {
        }

        public void AddRule(Mapping rule)
        {
            Rules.Add(rule);
        }
    }
}
