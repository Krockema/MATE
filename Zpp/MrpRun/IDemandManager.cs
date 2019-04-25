using System.Collections.Generic;
using Zpp.ModelExtensions;

namespace Zpp
{
    public interface IDemandManager
    {
        List<IDemand> orderByUrgency(List<IDemand> demands);
    }
}