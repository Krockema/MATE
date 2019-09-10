using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_Article : BaseEntity
    {
        public string Name { get; set; }

        [Display(Name = "Packing Unit")]
        public int UnitId { get; set; }
        public virtual M_Unit Unit { get; set; }
        [Display(Name = "Article Type")]
        public int ArticleTypeId { get; set; }
        [JsonIgnore]
        public virtual M_ArticleType ArticleType { get; set; }
        //[DisplayFormat(DataFormatString = "{0:0,0}")]
        // 
        public int? LotSize { get; set; }
        [DataType(DataType.Currency)]
        public double Price { get; set; }
        public int DeliveryPeriod { get; set; }
        [DataType(dataType: DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreationDate { get; set; }
        public M_Stock Stock { get; set; }
        [JsonIgnore]
        public virtual ICollection<M_ArticleBom> ArticleBoms { get; set; }
        [JsonIgnore]
        public virtual ICollection<M_ArticleBom> ArticleChilds { get; set; }
        [JsonIgnore]
        // public virtual IEnumerable<ArticleBomPart> ArticleChilds { get; set; } 
        public virtual ICollection<M_Operation> Operations { get; set; }
        [JsonIgnore]
        public virtual ICollection<T_ProductionOrder> ProductionOrders { get; set; }
        [JsonIgnore]
        public virtual ICollection<M_ArticleToBusinessPartner> ArticleToBusinessPartners { get; set;}
        public bool ToPurchase { get; set; }
        public bool ToBuild { get; set; }
        public string PictureUrl { get; set; }



        public IEnumerable<M_Article> AllChildren()
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

        public override string ToString()
        {
            return $"{Id.ToString()}: {Name}";
        }

    }
}