using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.Production.Core.Agents.HubAgent.Types
{
    internal class OperationPosition : Tuple<int, string>
    {
        public OperationPosition(int item1, string item2) : base(item1, item2)
        {

        }
    }
}
