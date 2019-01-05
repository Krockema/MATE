using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Journal
    {
        public string JournalId { get; set; }
        public long? ActionType { get; set; }
        public string Date { get; set; }
        public string Host { get; set; }
        public string ObjectId { get; set; }
        public string ObjecttypeId { get; set; }
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public string Value { get; set; }
    }
}
