using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.FindSymbols;
using Xunit;

namespace Master40.XUnitTest.Online.Agents.Types
{
    public class CapabilityManager
    {
        [Fact]
        public void TestSetCapabilies()
        {

        }

        [Fact]
        public void TestCapabilityRetrieval()
        {
            
        }

        /*

           SELECT RC.Id, RC.Name
           , RS.Name, RS.SetupTime
           , CR.Name,CR.Id
           , PR.Name,PR.Id
           FROM [TestContext].[dbo].[M_ResourceCapability] as RC
           JOIN dbo.M_ResourceSetup RS on RS.ResourceCapabilityId = RC.Id
           left JOIN dbo.M_Resource PR on PR.Id = RS.ParentResourceId
           left JOIN dbo.M_Resource CR on CR.Id = RS.ChildResourceId
           --WHERE RC.Id = 10005 --cutting
           WHERE RC.Id = 10008 --drilling

         */


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
