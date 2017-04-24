using System;
using System.ComponentModel.DataAnnotations;


namespace Master40.Models.DB
{
    public class ArticleType
    {
        [Key]
        public int ArticleTypeID { get; set; }
        public String Name { get; set; }
    }
}
