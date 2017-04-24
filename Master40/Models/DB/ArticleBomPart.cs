using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using System.Collections.Generic;

namespace Master40.Models.DB
{
    public class ArticleBomPart
    {
        [Key]
        public int ArticleBomPartsId { get; set; }
        public int? ParrentArticleBomPartId { get; set; }
        public virtual ArticleBomPart ParrentArticleBomPart { get; set; }
        public virtual ICollection<ArticleBomPart> ChildArticleBomParts { get; set; }
        public int ArticleId { get; set; }
        public virtual Article Article { get; set; }
        public int ArticleBomId { get; set; }
        public virtual ArticleBom ArticleBom { get; set; }
        public double Count { get; set; }
        public string Name { get; set; }
        public virtual ICollection<OperationChart> OperationCharts { get; set; }

    }
}