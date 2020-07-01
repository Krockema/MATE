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
        public M_ResourceCapability ResourceCapability { get; set; }
        public int ResourceId { get; set; }
        public M_Resource Resource { get; set; }
        public int ResourceToolId { get; set; }
        public M_ResourceTool ResourceTool { get; set; }
        /* Specific SetupTime for the ResourceTool to the Resource
         */
        public int SetupTime { get; set; }

    }
}
