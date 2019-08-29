using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Types;
using System;
using System.Linq;
using static FOperationResults;
using static FPostponeds;
using static FProposals;
using static FUpdateSimulationWorks;
using static FUpdateStartConditions;
using static IJobResults;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class Bucket : SimulationCore.Types.Behaviour
    {
        public Bucket(int planingJobQueueLength, int fixedJobQueueSize, WorkTimeGenerator workTimeGenerator, int resourceId, ToolManager toolManager, SimulationType simulationType = SimulationType.None) : base(childMaker: null, obj: simulationType)
        {
            this._processingQueue = new JobQueueItemLimited(limit: fixedJobQueueSize);
            this._planingQueue = new JobQueueTimeLimited(limit: planingJobQueueLength);
            this._agentDictionary = new AgentDictionary();
            _workTimeGenerator = workTimeGenerator;
            _resourceId = resourceId;
            _toolManager = toolManager;

        }
        // TODO Implement a JobManager
        internal JobQueueTimeLimited _planingQueue { get; set; }
        internal JobQueueItemLimited _processingQueue { get; set; }
        internal JobInProgress _jobInProgress { get; set; } = new JobInProgress();
        internal WorkTimeGenerator _workTimeGenerator { get; }
        internal int _resourceId { get; }
        internal ToolManager _toolManager { get; }
        internal AgentDictionary _agentDictionary { get; }
        

        public override bool Action(object message)
        {
            switch (message)
            {
                //case Resource.Instruction.SetHubAgent msg: SetHubAgent(hubAgent: msg.GetObjectFromMessage.Ref); break;
                //case Resource.Instruction.RequestProposal msg: RequestProposal(jobItem: msg.GetObjectFromMessage); break;
                //case Resource.Instruction.AcknowledgeProposal msg: AcknowledgeProposal(jobItem: msg.GetObjectFromMessage); break;
                //case BasicInstruction.UpdateStartConditions msg: UpdateStartCondition(startCondition: msg.GetObjectFromMessage); break;
                //case BasicInstruction.FinishJob msg: FinishJob(jobResult: msg.GetObjectFromMessage); break;
                // case BasicInstruction.ResourceBrakeDown msg: BreakDown((Resource)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }
    }
}
