using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace Mate.Production.AI
{
    internal class TransitionData
    {
        public float Label { get; set; }
        [VectorType(3)]
        public float[] Features { get; set; }
    }
}
