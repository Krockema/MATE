using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.ProductionAgent;
using Master40.SimulationCore.Agents.ResourceAgent;
using Master40.SimulationCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using static FBreakDowns;
using static FAgentInformations;
using static FOperationResults;
using static FOperations;
using static FProposals;
using static IJobs;
using static IJobResults;
using static FUpdateStartConditions;

namespace Master40.SimulationCore.Agents.HubAgent.Behaviour
{
    public class Bucket : SimulationCore.Types.Behaviour
    {
        internal Bucket(SimulationType simulationType = SimulationType.None)
                        : base(childMaker: null, obj: simulationType) { }


        internal List<FOperation> _operationList { get; set; } = new List<FOperation>();
        internal AgentDictionary _resourceAgents { get; set; } = new AgentDictionary();

        public override bool Action(object message)
        {
            switch (message)
            {
                //case Hub.Instruction.EnqueueJob msg: EnqueueJob(fOperation: msg.GetObjectFromMessage as FOperation); break;
                //case Hub.Instruction.ProposalFromResource msg: ProposalFromResource(fProposal: msg.GetObjectFromMessage); break;
                //case BasicInstruction.UpdateStartConditions msg: UpdateAndForwardStartConditions(msg.GetObjectFromMessage); break;
                //case BasicInstruction.WithdrawRequiredArticles msg: WithdrawRequiredArticles(operationKey: msg.GetObjectFromMessage); break;
                //case BasicInstruction.FinishJob msg: FinishJob(jobResult: msg.GetObjectFromMessage); break;
                //case Hub.Instruction.AddResourceToHub msg: AddResourceToHub(hubInformation: msg.GetObjectFromMessage); break;
                //case BasicInstruction.ResourceBrakeDown msg: ResourceBreakDown(breakDown: msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

    }
}
