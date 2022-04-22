using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.Production.AI
{
    public class DataPoint
    {
        public float Label { get; set; }
        [VectorType(3)]
        public float[] Features { get; set; }

    }
}
