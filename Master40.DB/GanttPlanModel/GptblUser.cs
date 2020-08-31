using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblUser
    {
        public string ClientId { get; set; }
        public string UserId { get; set; }
        public string Forename { get; set; }
        public string Password { get; set; }
        public int Status { get; set; }
        public string Surname { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
