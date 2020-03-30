using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.FindSymbols;
using Xunit;

namespace Master40.XUnitTest.Online.Agents.Types
{
    public class CapabilityManager
    {
        //Methode: Input Capability-Hierachy from "Cutting" with Sub-Capabilities

        //Methode:
        //Input Sub-Capability
        //Out List of Resource 1st Level with List of all subsequent resource levels with same capability (AgentRefs)
        // --> Proposal
        // Beispiel:
        /*         Refs für Resource Sägen 1
                        Resource Sägen 1
                        Resource Sägen 10mm 1
                   Refs für Resource Sägen 2
                        Resource Sägen 2
                        Resource Sägen 10mm 2
                   Refs für Wasserstrahlschneider 
                        Resource Wasser 1
        */

        // --> Methode: ProposalManager?

    }
}
