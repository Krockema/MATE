using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Master40.DB.Enums;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public interface IDemandToProvider
    {
        int Id { get; set; }
        int ArticleId { get; set; }
        M_Article Article { get; set; }
        decimal Quantity { get; set; }
        int? DemandRequesterId { get; set; }
        T_Demand DemandRequester { get; set; }
        List<T_Demand> DemandProvider { get; set; }
        State State { get; set; }

    }
}
