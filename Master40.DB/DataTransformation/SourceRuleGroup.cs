using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.Models;

namespace Master40.DB.DataTransformation
{
    public class SourceRuleGroup
    {
        public Dictionary<string, DestinationRuleGroup> RuleGroups { get; } = new Dictionary<string, DestinationRuleGroup>();

        public SourceRuleGroup()
        {
        }

        public void AddRuleGroup(string key, DestinationRuleGroup group)
        {
            RuleGroups.Add(key, group);
        }

        public bool IsAgentRuleGroup()
        {
            return RuleGroups.Values.First().IsAgentRuleGroup();
        }
    }
}
