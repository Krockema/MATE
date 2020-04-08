using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.DataModel
{
    /*
     * JOINTABLE Describes a combination of Resource and ResourceTool to provide a skill
     */

    public class M_ResourceSetup : BaseEntity
    {
        public string Name { get; set; }
        public int ResourceCapabilityId { get; set; }
        public bool UsedInSetup { get; set; }
        public bool UsedInProcess { get; set; }
        public M_ResourceCapability ResourceCapability { get; set; }
        public int? ChildResourceId { get; set; }
        public M_Resource ChildResource { get; set; }
        public int? ParentResourceId { get; set; }
        public M_Resource ParentResource { get; set; }
        /* Specific SetupTime for the ResourceTool to the Resource
         */
        public long SetupTime { get; set; }

    }
}
