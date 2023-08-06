using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Hive.Definitions;
using Mate.Production.Core.Agents;
using Mate.Production.Core.Agents.ResourceAgent.Types;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Environment.Records.Interfaces;
using Mate.Production.Core.Helper;
using Newtonsoft.Json;

namespace Mate.Production.Core.Reporting
{
    public static class GanttStatistics
    {
        public static void CreateGanttChartForRessource(JobInProgress jobInProgress, IJobQueue scopeQueue, Agent agent )
        {
            List<GanttChartItem> ganttData = new List<GanttChartItem>();
            if (jobInProgress.IsSet)
            {
                ganttData.AddRange(CreateGanttProcessingQueueLog(new[] { jobInProgress.GanttItem }, true, "Processing", jobInProgress, agent));
                // add from scope
            }
            ganttData.AddRange(CreateGanttProcessingQueueLog(jobInProgress.GanttItems.ToArray(), false, "ReadyElement", jobInProgress, agent));

            var jobs = scopeQueue.GetAllJobs().OrderBy(x => x.Job.Priority(agent.Time.Value)).ToList();

            ganttData.AddRange(CreateGanttProcessingQueueLog(jobs.ToArray(), false, "ScopeQueue", jobInProgress, agent));

            CustomFileWriter.WriteToFile($"Logs//ResourceScheduleAt-{agent.Time.Value}.log",
                JsonConvert.SerializeObject(ganttData).Replace("[", "").Replace("]", ","));


            if (jobs.Count > 0)
            {
                if (jobs.First().ScopeConfirmation.GetScopeStart() < jobInProgress.ResourceIsBusyUntil)
                {
                    agent.DebugMessage("Seems wrong");
                }
            }


        }

        private static List<GanttChartItem> CreateGanttProcessingQueueLog(IConfirmation[] jobArray, bool inProcessing, string source, JobInProgress jobInProgress, Agent agent)
        {
            var ganttTransformation = new List<GanttChartItem>();

            foreach (var bucket in jobArray)
            {
                DateTime operationStart = Time.ZERO.Value;
                var resourceName = agent.Name.Replace("Resource(", "").Replace(")", "");
                var usedInSetupOnly = !bucket.CapabilityProvider.ResourceSetups.Single(x => x.Resource.Name.Replace(" ", "").Equals(resourceName)).UsedInProcess;

                // Create Setup and Empty space after
                if (bucket.ScopeConfirmation.GetSetup() != null && agent.Time.Value <= bucket.ScopeConfirmation.GetSetup()?.End)
                {
                    var end = bucket.ScopeConfirmation.GetSetup()?.End;
                    operationStart = bucket.ScopeConfirmation.GetSetup().Start;
                    var setupDuration = bucket.ScopeConfirmation.GetSetup()?.End -
                                        bucket.ScopeConfirmation.GetSetup()?.Start;
                    var finalized = inProcessing;  //TODO: Not correct anymore - need to be fixed for fronend (Gantt chart)
                    var isWorking = false;

                    if (jobInProgress.SetupIsOngoing && bucket.Job.Key == jobInProgress.JobKey)
                    {
                        isWorking = true;
                        operationStart = agent.Time.Value;
                        var operationRemainingTime = setupDuration -
                                                     (agent.Time.Value - jobInProgress.LastTimeStartCall);
                        end = agent.Time.Value + operationRemainingTime;
                    }

                    ganttTransformation.Add(new GanttChartItem
                    {
                        article = $"{bucket.Job.Name} from {bucket.ScopeConfirmation.GetScopeStart()} to {bucket.ScopeConfirmation.GetScopeEnd()}",
                        articleId = bucket.Key.ToString(),
                        start = operationStart.ToString(),
                        end = end.ToString(),
                        groupId = bucket.Job.Name,
                        operation = $"Setup for {bucket.Job.Name} from {bucket.ScopeConfirmation.GetScopeStart()} to {bucket.ScopeConfirmation.GetScopeEnd()}",
                        operationId = bucket.Key.ToString(),
                        resource = agent.Name.Replace("Resource(", "").Replace(")", ""),
                        priority = bucket.Job.Priority(agent.Time.Value).ToString() + " S " + bucket.ScopeConfirmation.GetScopeStart() + " E " + bucket.ScopeConfirmation.GetScopeEnd(),
                        IsProcessing = jobInProgress.SetupIsOngoing.ToString(),
                        IsReady = ((BucketRecord)bucket.Job).HasSatisfiedJob().ToString(),
                        IsFinalized = finalized.ToString(),
                        IsWorking = isWorking.ToString(),
                        IsFixed = source
                    });
                    //End here if its a setup resource
                    if (usedInSetupOnly) continue;
                    //for Empty space of Main Resources between End Setup and Start Processing 
                    ganttTransformation.Add(new GanttChartItem
                    {
                        article = $"{bucket.Job.Name} from {bucket.ScopeConfirmation.GetScopeStart()} to {bucket.ScopeConfirmation.GetScopeEnd()}",
                        articleId = bucket.Key.ToString(),
                        start = end.ToString(),
                        end = bucket.ScopeConfirmation.GetProcessing().Start.ToString(), // bucket.ScopeConfirmation.GetScopeEnd().ToString(),
                        groupId = bucket.Job.Name,
                        operation = $"Empty bucket space for {bucket.Job.Name} waiting for operation Start",
                        operationId = bucket.Key.ToString(),
                        resource = agent.Name.Replace("Resource(", "").Replace(")", ""),
                        priority = bucket.Job.Priority(agent.Time.Value).ToString() + " S " + bucket.ScopeConfirmation.GetScopeStart() + " E " + bucket.ScopeConfirmation.GetScopeEnd(),
                        IsProcessing = false.ToString(),
                        IsReady = ((BucketRecord)bucket.Job).HasSatisfiedJob().ToString(),
                        IsFinalized = finalized.ToString(),
                        IsWorking = false.ToString(),
                        IsFixed = source
                    });

                }

                if (usedInSetupOnly) continue;
                if (inProcessing)
                {
                    CreateOperations(jobInProgress.OperationsAsArray(), bucket, ganttTransformation, inProcessing, source, jobInProgress, agent);
                }
                else
                {
                    var ops = ((BucketRecord)bucket.Job).Operations.OrderBy(x => x.Priority.Invoke(agent.Time.Value)).ToArray();
                    CreateOperations(ops, bucket, ganttTransformation, inProcessing, source, jobInProgress, agent);
                }

            }

            return ganttTransformation;
        }

        private static void CreateOperations(OperationRecord[] ops, IConfirmation bucket, List<GanttChartItem> ganttTransformation, bool inProcessing, string source, JobInProgress jobInProgress, Agent agent)
        {
            var operationStart = bucket.ScopeConfirmation.GetProcessing().Start;


            for (int j = 0; j < ops.Length; j++)
            {
                var operation = ops[j];
                var operationEnd = operationStart + operation.Operation.Duration;
                var isWorking = false;
                if (jobInProgress.CurrentOperation.Operation != null && jobInProgress.CurrentOperation.Operation.Key.Equals(operation.Key))
                {
                    isWorking = true;
                    operationStart = agent.Time.Value;
                    var operationRemainingTime = jobInProgress.CurrentOperation.Operation.Operation.Duration -
                                                 (agent.Time.Value - jobInProgress.LastTimeStartCall);
                    operationEnd = agent.Time.Value + operationRemainingTime;
                }

                ganttTransformation.Add(new GanttChartItem
                {
                    article = operation.Bucket,
                    articleId = operation.Key.ToString(),
                    start = operationStart.ToString(),
                    end = operationEnd.ToString(),
                    groupId = bucket.Job.Name,
                    operation = operation.Operation.Name,
                    operationId = operation.Operation.Id.ToString(),
                    resource = agent.Name.Replace("Resource(", "").Replace(")", ""),
                    priority = operation.Priority.Invoke(agent.Time.Value).ToString() + " S " + bucket.ScopeConfirmation.GetScopeStart() + " E " + bucket.ScopeConfirmation.GetScopeEnd(),
                    IsProcessing = inProcessing.ToString(),
                    IsReady = operation.StartConditions.Satisfied.ToString(),
                    IsFinalized = false.ToString(),
                    IsWorking = isWorking.ToString(),
                    IsFixed = source
                });
                operationStart = operationEnd;

                // add Empty space that is reserved
                if ((ops.Length - 1) == j && !inProcessing)
                {
                    ganttTransformation.Add(new GanttChartItem
                    {
                        article = $"{bucket.Job.Name} from {bucket.ScopeConfirmation.GetScopeStart()} to {bucket.ScopeConfirmation.GetScopeEnd()}",
                        articleId = bucket.Key.ToString(),
                        start = operationStart.ToString(),
                        end = bucket.ScopeConfirmation.GetScopeEnd().ToString(), // bucket.ScopeConfirmation.GetScopeEnd().ToString(),
                        groupId = bucket.Job.Name,
                        operation = $"Empty bucket space {bucket.Job.Name} from {bucket.ScopeConfirmation.GetScopeStart()} to {bucket.ScopeConfirmation.GetScopeEnd()}",
                        operationId = bucket.Key.ToString(),
                        resource = agent.Name.Replace("Resource(", "").Replace(")", ""),
                        priority = bucket.Job.Priority(agent.Time.Value).ToString() + " S " + bucket.ScopeConfirmation.GetScopeStart() + " E " + bucket.ScopeConfirmation.GetScopeEnd(),
                        IsProcessing = inProcessing.ToString(),
                        IsReady = false.ToString(),
                        IsFinalized = false.ToString(),
                        IsWorking = false.ToString(),
                        IsFixed = source
                    });
                }
            }
        }
    }
}
