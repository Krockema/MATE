namespace Mate.Production.Core.Environment.Abstractions
{
    public interface IOption<out T> 
    {
        T Value { get; }
    }
}