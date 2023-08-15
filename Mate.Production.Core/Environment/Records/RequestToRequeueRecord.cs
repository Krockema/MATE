using System;

namespace Mate.Production.Core.Environment.Records
{
    public record RequestToRequeueRecord( 
        Guid JobKey,
        bool Approved
    )
    {
        public RequestToRequeueRecord SetApproved => this with { Approved = true };
    }    
}
