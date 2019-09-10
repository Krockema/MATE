using System;

namespace Master40.SimulationCore.Environment.Abstractions
{
    public interface IOption<T> 
    {
        T Value { get; }
    }
}