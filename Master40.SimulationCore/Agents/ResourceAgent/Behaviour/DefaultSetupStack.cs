using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Helper.DistributionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using static FJobConfirmations;
using static FPostponeds;
using static FProposals;
using static FRequestProposalForCapabilityProviders;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class DefaultSetupStack : DefaultSetup
    {
        public DefaultSetupStack(int planingJobQueueLength
            , int fixedJobQueueSize
            , WorkTimeGenerator workTimeGenerator
            , List<M_ResourceCapabilityProvider> capabilityProvider
            , SimulationType simulationType = SimulationType.None) 
            : base(simulationType: simulationType
                , planingJobQueueLength: planingJobQueueLength
                , fixedJobQueueSize: fixedJobQueueSize
                , workTimeGenerator: workTimeGenerator
                , capabilityProvider: capabilityProvider)
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

        internal override void SendProposalTo(FRequestProposalForCapabilityProvider requestProposal)
        {
            //Get SetupDuration depending on ??? 
            var setupDuration = GetSetupTime(requestProposal.CapabilityProviderId);

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
                , capabilityProviderId: requestProposal.CapabilityProviderId
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
                _planingQueue.RemoveJob(job);
                Agent.Send(instruction: Hub.Instruction.Default.EnqueueJob.Create(job.Job, target: job.Job.HubAgent));
            }
            Agent.DebugMessage(msg: "New planning queue length = " + _planingQueue.Count);
        }
    }
}
