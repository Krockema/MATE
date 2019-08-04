using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.Agents.Agents.DataTransformation
{
    [Flags]
    public enum PropertyType : byte
    {
        None = 0,
        Id = 1,             // Property is used to identify an object
        Retransform = 2,    // Property has needs to be overwritten with new value
    }
}
