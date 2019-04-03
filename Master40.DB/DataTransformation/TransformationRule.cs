using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.DataTransformation
{
    public class TransformationRule
    {
        string Source { get; }
        string Destination { get; }
        public TransformationRule(string source, string destination)
        {
            this.Source = source;
            this.Destination = destination;
        }
    }
}
