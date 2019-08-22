using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.DataModel
{
    /*
     * Previous called MachineGroup, now describes a Skill to do something
     * owns a list of Resources 
     */
    public class M_ResourceSkill : BaseEntity
    {
        public string Name { get; set; }
        /* 
         * Resources, who can provide the required ResourceSkill
         */
        [JsonIgnore]
        public virtual ICollection<M_ResourceSetup> ResourceSetups { get; set; }


    }
}
