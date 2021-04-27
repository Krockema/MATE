using System.Collections.Generic;

namespace Mate.DataCore.ReportingModel
{
    public class ConfigurationItem : ResultBaseEntity
    { 
        public string Property { get; set; }
        public string PropertyValue { get; set; }
        public string Description { get; set; }
        public ICollection<ConfigurationRelation> ParentItems { get; set; }
        public ICollection<ConfigurationRelation> ChildItems { get; set; }
        /* for later use
        
        public string From { get; set; }
        public string To { get; set; }
        public string Steps { get; set; }
        public bool IsIterable { get; set; }
        public string Type { get; set; }
        public string Operator { get; set; }

        */
    }
}
