using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.Production.Core.Environment.Records.Scopes
{
    public record PostponedRecord
    (
        TimeSpan Offset
    )
    {
        public bool IsPostponed => this.Offset > TimeSpan.Zero;
    }
}
