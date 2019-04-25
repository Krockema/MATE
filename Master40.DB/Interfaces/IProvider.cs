using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Newtonsoft.Json;

namespace Master40.DB.Interfaces
{
    public interface IProvider 
    {
        int Id { get; set; }
        int ArticleId { get; set; }
        M_Article Article { get; set; }
        decimal Quantity { get; set; }
        int? DemandRequesterId { get; set; }
        T_DemandToProvider DemandRequester { get; set; }
        List<T_DemandToProvider> DemandProvider { get; set; }
        State State { get; set; }
    }
}