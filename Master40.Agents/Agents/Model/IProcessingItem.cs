using System;
using System.Collections.Generic;

namespace Master40.Agents.Agents.Model
{
    internal interface IProcessingItem
    {
        Guid Id { get; }
        List<Proposal>  Proposals { get; }
        double Priority(long now);
        double Priority();
    }
}