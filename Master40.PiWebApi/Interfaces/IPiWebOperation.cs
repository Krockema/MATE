using System.Collections.Generic;

namespace Master40.PiWebApi.Interfaces
{
    public interface IPiWebOperation
    {
        string Name { get; }
        int ResourceCapabilityId { get; }
        IEnumerable<IPiWebCharacteristic> Characteristics { get; }
    }
}