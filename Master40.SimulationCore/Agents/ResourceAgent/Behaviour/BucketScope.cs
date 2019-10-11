using Master40.DB.Enums;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.DistributionProvider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Master40.DB.Data.Helper;
using static FBucketScopes;
using static FUpdateBucketScopes;

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
                    RequestProposalForBucketScope(msg.GetObjectFromMessage); break;
                case Resource.Instruction.BucketScope.UpdateBucketScope msg:
                    UpdateBucketScope(msg.GetObjectFromMessage); break;
                case Resource.Instruction.BucketScope.AcknowledgeBucketScope msg:
                    AcknowledgeBucketScope(msg.GetObjectFromMessage); break;
                default:
                    success = base.Action(message);
                    break;
            }
            return success;
        }

        internal void RequestProposalForBucketScope(FBucketScope bucketScope)
        {
            //Recieves a new bucket scope

            var scope = _bucketScopeQueue.GetQueueableScope(bucketScope);


        }

        internal void AcknowledgeBucketScope(Guid bucketKey)
        {


        }


        internal void UpdateBucketScope(FBucketScope updateBucketScope)
        {

        }

        

    }
}
