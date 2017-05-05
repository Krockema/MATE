using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using System.Collections.Generic;

namespace Master40.Models.DB
{
    public class Article
    {

        [Key]
        public int ArticleId { get; set; }
        public string Name { get; set; }

        [Display(Name = "Packing Unit")]
        public int UnitId { get; set; }
        public virtual Unit Unit { get; set; }
        [Display(Name = "Article Type")]
        public int ArticleTypeId { get; set; }
        public virtual ArticleType ArticleType { get; set; }
        //[DisplayFormat(DataFormatString = "{0:0,0}")]
        // 
        [DataType(DataType.Currency)]
        public double Price { get; set; }
        public int DeliveryPeriod { get; set; }
        [DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreationDate { get; set; }
        public Stock Stock { get; set; }
        public virtual ICollection<ArticleBom> ArticleBoms { get; set; }
        public virtual ICollection<ArticleBom> ArticleChilds { get; set; }
        // public virtual IEnumerable<ArticleBomPart> ArticleChilds { get; set; } 
        public virtual ICollection<ArticleToWorkSchedule> ArticleToWorkSchedules { get; set; }
        public virtual ICollection<ProductionOrder> ProductionOrders { get; set; }
        public virtual ICollection<ArticleToDemand> ArtilceToDemand { get; set; }

    }
}