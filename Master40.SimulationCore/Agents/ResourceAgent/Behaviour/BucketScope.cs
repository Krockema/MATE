using Master40.DB.Enums;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.DistributionProvider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Master40.DB.Data.Helper;
using static FBucketScopes;
using static FUpdateBucketScopes;
using Master40.SimulationCore.Agents.HubAgent;
using static FBuckets;
using static FUpdateStartConditions;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class BucketScope : DefaultSetup
    {
        public BucketScope(int planingJobQueueLength, int fixedJobQueueSize, WorkTimeGenerator workTimeGenerator, ToolManager toolManager, SimulationType simulationType = SimulationType.None)
            : base(simulationType: simulationType
        , planingJobQueueLength: planingJobQueueLength
        , fixedJobQueueSize: fixedJobQueueSize
        , workTimeGenerator: workTimeGenerator
        , toolManager: toolManager)
        {
        }

        private BucketScopeQueue _bucketScopeQueue = new BucketScopeQueue(limit: 1000L);

        public override bool Action(object message)
        {
            var success = true;
            switch (message)
            {
                case Resource.Instruction.BucketScope.RequestProposalForBucketScope msg:
                    RequestProposalForBucketScope(msg.GetObjectFromMessage as FBucket); break;
                case Resource.Instruction.BucketScope.AcknowledgeBucketScope msg:
                    AcknowledgeBucketScope(msg.GetObjectFromMessage); break;
                case BasicInstruction.UpdateStartConditions msg:
                    UpdateStartConditions(msg.GetObjectFromMessage);
                    break;
                default:
                    success = base.Action(message);
                    break;
            }
            return success;
        }

        private void UpdateStartConditions(FUpdateStartCondition startCondition)
        {
            _planingQueue.UpdatePreConditionForOperation(startCondition);

        }

        internal void RequestProposalForBucketScope(FBucket bucket)
        {
            //Recieves a new bucket scope
            var bucketScope = new Types.BucketScope(bucketKey: bucket.Key
                                                    , bucketStart: bucket.ForwardStart
                                                    , bucketEnd: bucket.BackwardStart
                                                    , duration: ((IJobs.IJob)bucket).Duration);

            
            
            //GetQueableTime




        }

        internal void AcknowledgeBucketScope(Guid bucketKey)
        {


        }


        internal override void UpdateProcessingQueue()
        {
            // take the next scope and make it fix 
            while (_processingQueue.CapacitiesLeft() && _bucketScopeQueue.HasQueueAbleJobs())
            {
                var job = _planingQueue.DequeueFirstSatisfied(currentTime: Agent.CurrentTime);
                Agent.DebugMessage(msg: $"Job to place in processingQueue: {job.Key} {job.Name} Try to start processing.");
                var ok = _processingQueue.Enqueue(item: job);
                if (!ok)
                {
                    throw new Exception(message: "Something wen wrong with Queueing!");
                }
                Agent.DebugMessage(msg: $"Start withdraw for article {job.Name} {job.Key}");
                Agent.Send(instruction: BasicInstruction.WithdrawRequiredArticles.Create(message: job.Key, target: job.HubAgent));
            }

            Agent.DebugMessage(msg: $"Jobs ready to start: {_processingQueue.Count} Try to start processing.");


        }
        
    }
}
