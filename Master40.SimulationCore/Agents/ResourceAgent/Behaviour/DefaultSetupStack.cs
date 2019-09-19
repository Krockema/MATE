using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.DistributionProvider;
using Master40.SimulationCore.Types;
using static FPostponeds;
using static FProposals;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class DefaultSetupStack : DefaultSetup
    {
        public DefaultSetupStack(int planingJobQueueLength
            , int fixedJobQueueSize
            , WorkTimeGenerator workTimeGenerator
            , ToolManager toolManager
            , SimulationType simulationType = SimulationType.None) 
            : base(simulationType: simulationType
                , planingJobQueueLength: planingJobQueueLength
                , fixedJobQueueSize: fixedJobQueueSize
                , workTimeGenerator: workTimeGenerator
                , toolManager: toolManager)
        {

        }
     
        public override bool Action(object message)
        {
            bool success = true;
            switch (message)
            {
                default: success = base.Action(message);
                    break;
            }
            return success;
        }

        internal override void UpdateProcessingQueue()
        {
            while (_processingQueue.CapacitiesLeft() && _planingQueue.HasQueueAbleJobs())
            {
                var jobs = _planingQueue.GetAllSatisfiedSameTool(currentTime: Agent.CurrentTime);

                Agent.DebugMessage(msg: $"{jobs.Count} jobs for {jobs.FirstOrDefault().Tool.Name} have been placed in processing queue");

                if (!_processingQueue.EnqueueAll(jobs))
                {
                    throw new Exception(message: "Something went wrong with Queueing!");
                }

                foreach (var job in jobs)
                {
                    Agent.DebugMessage(msg: $"WithdrawRequiredArticles for {job.Name} {job.Key}");
                    Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles.Create(message: job.Key, target: job.HubAgent));
                }
            }
            Agent.DebugMessage(msg: $"Jobs ready to start: {_processingQueue.Count} Try to start processing.");
        }

        internal override void SendProposalTo(IJobs.IJob jobItem)
        {
            var setupDuration = GetSetupTime(jobItem: jobItem);

            var queuePosition = _planingQueue.GetQueueAbleTime(job: jobItem
                , currentTime: Agent.CurrentTime
                , resourceIsBlockedUntil: _jobInProgress.ResourceIsBusyUntil
                , processingQueueLength: _processingQueue.SumDurations
                , setupDuration: setupDuration);

            if (queuePosition.IsQueueAble)
            {
                Agent.DebugMessage(msg: $"IsQueueable: {queuePosition.IsQueueAble} with EstimatedStart: {queuePosition.EstimatedStart}");
            }
            var fPostponed = new FPostponed(offset: queuePosition.IsQueueAble ? 0 : _planingQueue.Limit);

            if (fPostponed.IsPostponed)
            {
                Agent.DebugMessage(msg: $"Postponed: { fPostponed.IsPostponed } with Offset: { fPostponed.Offset} ");
            }
            // calculate proposal
            var proposal = new FProposal(possibleSchedule: queuePosition.EstimatedStart
                , postponed: fPostponed
                , resourceAgent: Agent.Context.Self
                , jobKey: jobItem.Key);

            Agent.Send(instruction: Hub.Instruction.ProposalFromResource.Create(message: proposal, target: Agent.Context.Sender));
        }
    }
}
