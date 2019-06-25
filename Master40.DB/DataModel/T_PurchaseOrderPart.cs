using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class T_PurchaseOrderPart : BaseEntity, IProvider
    {
        public int PurchaseOrderId { get; set; }
        [JsonIgnore]
        public T_PurchaseOrder PurchaseOrder { get; set; }
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        public int Quantity { get; set; }
        [JsonIgnore]
        public State State { get; set; }
        
        public int ProviderId { get; set; }
        public T_Provider Provider { get; set; }

        public T_PurchaseOrderPart()
        {
            // it must be always a T_Provider created for every IProvider
            Provider = new T_Provider();
        }
        
        public int GetDueTime()
        {
            return PurchaseOrder.DueTime;
        }
        
        public M_Article GetArticle()
        {
            return Article;
        }
        
        public Quantity GetQuantity()
        {
            return new Quantity(Quantity);
        }
    }
}
