using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblAttachment
    {
        public string ClientId { get; set; }
        public string AttachmentId { get; set; }
        public string Name { get; set; }
        public string ObjecttypeId { get; set; }
        public string ObjectId { get; set; }
        public string Path { get; set; }
        public int? Type { get; set; }
        public string Value { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
