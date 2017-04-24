using System.ComponentModel.DataAnnotations;

namespace Master40.Models.DB
{
    public class Stock
    {
        [Key]
        public int StockID { get; set; }

        public string Name { get; set; }
        public decimal Max { get; set; }
        public decimal Min { get; set; }
        public decimal Current { get; set; }

        public int ArticleForeignKey { get; set; }
        public Article Article { get; set; }
    }
}
