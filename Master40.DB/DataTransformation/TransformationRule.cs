using System;
using System.Reflection;

namespace Master40.DB.DataTransformation
{
    public class TransformationRule
    {
        string Source { get; }
        string Destination { get; }
        public TransformationRule(string source, string destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}
