using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblJournal
    {
        public string ClientId { get; set; }
        public long JournalId { get; set; }
        public int? ActionType { get; set; }
        public DateTime? Date { get; set; }
        public string Host { get; set; }
        public string ObjectId { get; set; }
        public string ObjecttypeId { get; set; }
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public string Value { get; set; }
    }
}
