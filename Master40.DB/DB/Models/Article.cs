using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
namespace Master40.DB.Models
{
    public class Article : BaseEntity
    {
        public string Name { get; set; }

        [Display(Name = "Packing Unit")]
        public int UnitId { get; set; }
        public virtual Unit Unit { get; set; }
        [Display(Name = "Article Type")]
        public int ArticleTypeId { get; set; }
        [JsonIgnore]
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
        [JsonIgnore]
        public virtual ICollection<ArticleBom> ArticleBoms { get; set; }
        [JsonIgnore]
        public virtual ICollection<ArticleBom> ArticleChilds { get; set; }
        [JsonIgnore]
        // public virtual IEnumerable<ArticleBomPart> ArticleChilds { get; set; } 
        public virtual ICollection<WorkSchedule> WorkSchedules { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProductionOrder> ProductionOrders { get; set; }
        [JsonIgnore]
        public virtual ICollection<DemandToProvider> DemandToProviders { get; set; }
        [JsonIgnore]
        public virtual ICollection<ArticleToBusinessPartner> ArticleToBusinessPartners { get; set;}
        public bool ToPurchase { get; set; }
        public bool ToBuild { get; set; }
        public string PictureUrl { get; set; }



        public IEnumerable<Article> AllChildren()
        {
            yield return this;
            foreach (var child in ArticleChilds)
                if (child.ArticleChild.ArticleBoms != null)
                {
                    foreach (var granChild in child.ArticleChild.ArticleBoms)
                    {
                        foreach (var item in granChild.ArticleChild.AllChildren())
                        {
                            yield return item;
                        }
                    }
                }

        }

    }
}