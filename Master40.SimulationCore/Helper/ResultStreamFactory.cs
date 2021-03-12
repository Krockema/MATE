using System;
using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents;
using static FUpdateSimulationJobs;
using static FCreateSimulationResourceSetups;
using static FArticles;
using static FUpdateSimulationWorkProviders;
using static IJobs;
using static FBucketResults;
using static IConfirmations;

namespace Master40.SimulationCore.Helper
{
    public static class ResultStreamFactory
    {

        public static void PublishJob(Agent agent, IJob job, long duration, M_ResourceCapabilityProvider capabilityProvider)
        {
            PublishJob(agent, job, duration, capabilityProvider, job.Bucket);
        }

        public static void PublishJob(Agent agent, IJob job, long duration, M_ResourceCapabilityProvider capabilityProvider, string bucketName)
        {
            var pub = new FUpdateSimulationJob(job: job
                , duration: duration
                , start: agent.CurrentTime
                , capabilityProvider: capabilityProvider.Name
                , jobType: JobType.OPERATION
                , bucket: bucketName
                , setupId: capabilityProvider.Id);
            agent.Context.System.EventStream.Publish(@event: pub);
        }

        public static void PublishResourceSetup(Agent agent, long duration, M_ResourceCapabilityProvider capabilityProvider)
        {
            var pubSetup = new FCreateSimulationResourceSetup(expectedDuration: duration
                , duration: duration
                , start: agent.CurrentTime
                , capabilityProvider: capabilityProvider.Name
                , capabilityName: capabilityProvider.Name
                , setupId: capabilityProvider.Id);
            agent.Context.System.EventStream.Publish(@event: pubSetup);
        }

        public static void PublishUpdateArticleProvider(Agent agent, FArticle article)
        {
            var pub = new FUpdateSimulationWorkProvider(fArticleProviderKeys: article.ProviderList
                , requestAgentId: article.OriginRequester.Path.Uid.ToString()
                , requestAgentName: article.OriginRequester.Path.Name
                //, originRequesterId: article.OriginRequester.Path.Uid.ToString()
                //, originRequesterName: article.OriginRequester.Path.Uid.ToString()
                , isHeadDemand: article.IsHeadDemand
                , customerOrderId: article.CustomerOrderId);
            agent.Context.System.EventStream.Publish(@event: pub);
        }

        public static void PublishBucketResult(Agent agent, IConfirmation jobConfirmation, long start)
        {
            var pub = new FBucketResult(key: jobConfirmation.Job.Key
                , creationTime: 0
                , start: start
                , end: agent.CurrentTime
                , originalDuration: jobConfirmation.Job.Duration
                , productionAgent: ActorRefs.Nobody
                , capabilityProvider: jobConfirmation.CapabilityProvider.Name);

            //TODO NO tracking
            agent.Context.System.EventStream.Publish(@event: pub);
        }
    }
}
