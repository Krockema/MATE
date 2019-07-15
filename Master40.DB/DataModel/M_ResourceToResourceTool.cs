using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.DataModel
{
    /*
     * JOINTABLE Describes a combination of Resource and ResourceTool to provide a skill
     */

    public class M_ResourceToResourceTool : BaseEntity
    {
        public string Name { get; set; }
        public int ResourceId { get; set; }
        public M_Resource Resource { get; set; }
        public int ResourceToolId { get; set; }
        public M_ResourceTool ResourceTool { get; set; }
        /* Specific SetupTimefor the ResourceTool to the Resource
         */
        public int SetupTime { get; set; }

    }
}
