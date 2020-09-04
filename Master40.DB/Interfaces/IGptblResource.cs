using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.Interfaces
{
    public interface IGptblResource
    {
        string Id { get; }
        string ClientId { get; }
        string Name { get; }
    }
}
