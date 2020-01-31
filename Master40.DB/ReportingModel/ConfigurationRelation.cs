using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Master40.DB.ReportingModel
{
    public class ConfigurationRelation : ResultBaseEntity
    {
        public int ParentItemId { get; set; }
        public ConfigurationItem ParentItem { get; set; }
        public int ChildItemId { get; set; }
        public ConfigurationItem ChildItem { get; set; }
    }
}
