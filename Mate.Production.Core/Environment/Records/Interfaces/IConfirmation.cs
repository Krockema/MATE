using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment.Records.Scopes;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.Production.Core.Environment.Records.Interfaces
{
    public interface IConfirmation
    {
        IJob Job { get; }
        Guid Key { get; }
        TimeSpan Duration { get; }
        ScopeConfirmationRecord ScopeConfirmation { get; }
        M_ResourceCapabilityProvider CapabilityProvider { get; }
        IActorRef JobAgentRef { get; }
        public bool IsReset { get; }
        public IConfirmation UpdateJob(IJob job);
    }
}
