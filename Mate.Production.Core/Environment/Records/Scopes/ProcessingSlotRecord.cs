using Mate.Production.Core.Environment.Records.Interfaces;
using System;

namespace Mate.Production.Core.Environment.Records.Scopes
{
    public record ProcessingSlotRecord
    (
        DateTime Start,
        DateTime End): ITimeRange; 
    
}
