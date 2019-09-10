using System.Collections;
using System.Collections.Generic;
using Master40.DB.Interfaces;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_Operation : BaseEntity, IOperation
    {
        public int HierarchyNumber { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public int AverageTransitionDuration { get; set; }
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        public int ResourceSkillId { get; set; }
        public M_ResourceSkill ResourceSkill { get; set; }
        public ICollection<M_ArticleBom> ArticleBoms { get; set; }

        /// <summary>
        /// TODO Probably necessary to add multiply skills to a hub
        /// </summary>
        public int ResourceToolId { get; set; }
        public M_ResourceTool ResourceTool { get; set; }
    }
}