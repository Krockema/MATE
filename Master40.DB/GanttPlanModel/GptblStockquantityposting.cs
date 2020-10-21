using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblStockquantityposting
    {
        public string ClientId { get; set; }
        public string StockquantitypostingId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public DateTime? Date { get; set; }
        public string InfoObjectId { get; set; }
        public string InfoObjecttypeId { get; set; }
        public string MaterialId { get; set; }
        public double? Quantity { get; set; }
        public string QuantityUnitId { get; set; }
        public int? PostingType { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
