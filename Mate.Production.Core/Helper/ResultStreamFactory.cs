using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Agents;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Environment.Records.Interfaces;
using Mate.Production.Core.Environment.Records.Reporting;
using System;
using System.Collections.Immutable;


namespace Mate.Production.Core.Helper
{
    public static class ResultStreamFactory
    {

        public static void PublishJob(Agent agent, IJob job, TimeSpan duration, M_ResourceCapabilityProvider capabilityProvider)
        {
            PublishJob(agent, job, duration, capabilityProvider, job.Bucket);
        }

        public static void PublishJob(Agent agent, IJob job, TimeSpan duration, M_ResourceCapabilityProvider capabilityProvider, string bucketName)
        {
            var pub = new UpdateSimulationJobRecord(Job: job
                , Duration: duration
                , Start: agent.Time.Value
                , CapabilityProvider: capabilityProvider.Name
                , Capability: capabilityProvider.ResourceCapability.Name
                , ReadyAt: job.StartCondition.WasSetReadyAt
                , JobType: JobType.OPERATION
                , Bucket: bucketName
                , SetupId: capabilityProvider.Id);
            agent.Context.System.EventStream.Publish(@event: pub);
        }

        public static void PublishResourceSetup(Agent agent, TimeSpan duration, M_ResourceCapabilityProvider capabilityProvider)
        {
            var pubSetup = new CreateSimulationResourceSetupRecord(ExpectedDuration: duration
                , Duration: duration
                , Start: agent.Time.Value
                , CapabilityProvider: capabilityProvider.Name
                , CapabilityName: capabilityProvider.Name
                , SetupId: capabilityProvider.Id);
            agent.Context.System.EventStream.Publish(@event: pubSetup);
        }

        public static void PublishUpdateArticleProvider(Agent agent, ArticleRecord article)
        {
            var pub = new UpdateSimulationWorkProviderRecord(ArticleProviderRecords: article.ProviderList.ToImmutableHashSet()
                , RequestAgentId: article.OriginRequester.Path.Uid.ToString()
                , RequestAgentName: article.OriginRequester.Path.Name
                //, originRequesterId: article.OriginRequester.Path.Uid.ToString()
                //, originRequesterName: article.OriginRequester.Path.Uid.ToString()
                , IsHeadDemand: article.IsHeadDemand
                , CustomerOrderId: article.CustomerOrderId);
            agent.Context.System.EventStream.Publish(@event: pub);
        }

        public static void PublishBucketResult(Agent agent, IConfirmation jobConfirmation, DateTime start)
        {
            var pub = new BucketResultRecord(Key: jobConfirmation.Job.Key
                , CreationTime: Time.ZERO.Value
                , Start: start
                , End: agent.Time.Value
                , OriginalDuration: jobConfirmation.Job.Duration
                , ProductionAgent: ActorRefs.Nobody
                , CapabilityProvider: jobConfirmation.CapabilityProvider.Name);

            //TODO NO tracking
            agent.Context.System.EventStream.Publish(@event: pub);
        }
    }
}
