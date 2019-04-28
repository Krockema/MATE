using System.Collections.Generic;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class T_CustomerOrderPart : BaseEntity, IDemand
    {
        public int OrderId { get; set; }
        [JsonIgnore]
        public T_CustomerOrder CustomerOrder { get; set; }
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        public int Quantity { get; set; }
        [JsonIgnore]
        public bool IsPlanned { get; set; }
        public State State { get; set; }
        
        /*
        [NotMapped]
        public int RequesterId { get => this.OrderPartId; }
        public DemandToProvider DemandToDemand { get; set; }
        [NotMapped]
        public string Source { get; private set; }
        */
        public int DemandID { get; set; }
        public T_Demand Demand { get; set; }
        
        public int getDueTime()
        {
            return CustomerOrder.DueTime;
        }
    }

}


