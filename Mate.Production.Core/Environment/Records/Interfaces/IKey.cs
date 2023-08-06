using System;
namespace Mate.Production.Core.Environment.Records.Interfaces
{
    public interface IKey
    {
        public Guid Key { get => Key; }
        public DateTime CreationTime { get; }
    }
}
