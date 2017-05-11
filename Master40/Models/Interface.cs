using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models
{
    interface IDemand
    {
        int RequesterId { get; }
        string Source { get; }
    }


    interface IProvider
    {
        int ProviderId { get; }
        string Source { get; }
    }
}
