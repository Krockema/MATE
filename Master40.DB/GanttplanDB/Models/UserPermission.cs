using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class UserPermission
    {
        public string UserId { get; set; }
        public string ObjecttypeId { get; set; }
        public long Permissiontype { get; set; }
    }
}
