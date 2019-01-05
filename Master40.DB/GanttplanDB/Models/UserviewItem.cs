using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class UserviewItem
    {
        public string UserviewId { get; set; }
        public string ItemId { get; set; }
        public string ItemType { get; set; }
        public long? ItemIndex { get; set; }
    }
}
