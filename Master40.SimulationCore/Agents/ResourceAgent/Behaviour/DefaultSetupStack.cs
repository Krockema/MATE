using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using System;
using System.Linq;
using Master40.SimulationCore.Helper.DistributionProvider;
using static FPostponeds;
using static FProposals;
using static FRequestProposalForSetups;
using static FJobConfirmations;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class DefaultSetupStack : DefaultSetup
    {
        public DefaultSetupStack(int planingJobQueueLength
            , int fixedJobQueueSize
            , WorkTimeGenerator workTimeGenerator
            , SetupManager toolManager
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
                var jobConfirmations = _planingQueue.GetAllSatisfiedSameCapability(currentTime: Agent.CurrentTime);

                Agent.DebugMessage(msg: $"{jobConfirmations.Count} jobs for {jobConfirmations.FirstOrDefault().Job.RequiredCapability.Name} have been placed in processing queue");

                if (!_processingQueue.EnqueueAll(jobConfirmations.ToList()))
                {
                    throw new Exception(message: "Something went wrong with Queueing!");
                }

                foreach (var jobConfirmation in jobConfirmations)
                {
                    Agent.DebugMessage(msg: $"WithdrawRequiredArticles for {jobConfirmation.Job.Name} {jobConfirmation.Job.Key}");
                    Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles.Create(message: jobConfirmation.Job.Key, target: jobConfirmation.Job.HubAgent));
                }
            }
            Agent.DebugMessage(msg: $"Jobs ready to start: {_processingQueue.Count} Try to start processing.");
        }

        internal override void SendProposalTo(FRequestProposalForSetup requestProposal)
        {
            //Get SetupDuration depending on ??? 
            var setupDuration = GetSetupTime(jobItem: requestProposal.Job);

            var queuePosition = _planingQueue.GetQueueAbleTimeByStack(job: requestProposal.Job
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
                , setupId: requestProposal.SetupId
                , resourceAgent: Agent.Context.Self
                , jobKey: requestProposal.Job.Key);

            Agent.Send(instruction: Hub.Instruction.Default.ProposalFromResource.Create(message: proposal, target: Agent.Context.Sender));
        }

        internal override void UpdateAndRequeuePlanedJobs(FJobConfirmation jobConfirmation)
        {
            Agent.DebugMessage(msg: "Old planning queue length = " + _planingQueue.Count);
            var toRequeue = _planingQueue.CutTailByStack(currentTime: Agent.CurrentTime, jobConfirmation);
            foreach (var job in toRequeue)
            {
                _planingQueue.RemoveJob(jobConfirmation);
                Agent.Send(instruction: Hub.Instruction.Default.EnqueueJob.Create(message: job.Job, target: jobConfirmation.Job.HubAgent));
            }
            Agent.DebugMessage(msg: "New planning queue length = " + _planingQueue.Count);
        }
    }
}
