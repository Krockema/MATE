using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.Enums;

namespace Master40.DB.Models
{
    public class StockExchange : BaseEntity
    {
        public int StockId { get; set; }
        public Stock Stock { get; set; }
        public int RequiredOnTime { get; set; }
        public int Quantity { get; set; }
        public EchangeType EchangeType { get; set; }

    }
}
