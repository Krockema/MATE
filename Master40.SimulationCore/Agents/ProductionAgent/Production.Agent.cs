using Akka.Actor;
using Master40.DB.Models;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;
using static Master40.SimulationCore.Agents.Production.Properties;

namespace Master40.SimulationCore.Agents
{
    public partial class Production : Agent
    {
        internal IActorRef Guardian => this.ActorPaths.Guardians.Single(x => x.Key == GuardianType.Dispo).Value;
        /*
        private RequestItem _requestItem;
        private List<WorkItem> _workItems { get; set; }
        private List<RequestItem> _requestItems { get; set; }
        private Dictionary<IActorRef, string> _hubAgents;
        private ElementStatus _status;
        private WorkItem _nextItem {get; set;}
        */
        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(() => new Production(actorPaths, time, debug, principal));
        }

        public Production(ActorPaths actorPaths, long time, bool debug, IActorRef principal) : base(actorPaths, time, debug, principal)
        {
            DebugMessage("Woke up.");
        }

        internal void Finished(ItemStatus status)
        {
            // // any Not Finished do noting
            // if (ChildAgents.Any(x => x.Status != Status.Finished))
            //     return;
            var workItems = Get<List<WorkItem>>(WORK_ITEMS);
            var requestItem = Get<RequestItem>(REQUESTED_ITEMS);
            var hubAgents = Get<Dictionary<IActorRef, string>>(HUB_AGENTS);
            // Return from Production as WorkItemStatus
            if (status != null)
            {
                var workItem = workItems.First(x => x.Key == status.ItemId);
                workItems.Replace(workItem.UpdateStatus(status.Status));
            }

            // TODO Anything ?
            if (requestItem.Article.WorkSchedules != null && workItems.All(x => x.Status == ElementStatus.Finished))
            {
                this.Status = ElementStatus.Finished;
                Send(instruction: Storage.Instruction.ResponseFromProduction.Create(requestItem, requestItem.StorageAgent));

                DebugMessage("All Workschedules have been Finished");
                base.Finish();
                return;
            }
            // else
            DebugMessage("Im Ready To get Enqued");
            Status = (Status == ElementStatus.Processed) ? ElementStatus.Processed : ElementStatus.Ready;
            workItems.ForEach(item => item = item.UpdateMaterialsProvided(true));
            if (hubAgents.Count > 0)
            {
                SetWorkItemReady();
            }
        }
    

        internal void RequestHubAgentFor(ICollection<WorkSchedule> workSchedules)
        {
            // Request Comunication Agent for my Workschedules
            var machineGroups = workSchedules.Select(x => x.MachineGroup.Name).Distinct().ToList();
            foreach (var machineGroupName in machineGroups)
            {   
                Send(Directory.Instruction
                            .RequestRessourceAgent
                            .Create(descriminator: machineGroupName
                                    ,target: ActorPaths.HubDirectory.Ref));
            }
        }

        internal void CreateWorkItemsFromRequestItem(bool firstItemToBuild, RequestItem requestItem)
        {
            var workItems =  Get<List<WorkItem>>(WORK_ITEMS);
            var lastDue = requestItem.DueTime;
            foreach (var workSchedule in requestItem.Article.WorkSchedules.OrderBy(x => x.HierarchyNumber))
            {
                var n = workSchedule.ToWorkItem(dueTime: lastDue
                                                    , status: firstItemToBuild ? ElementStatus.Ready : ElementStatus.Created
                                                    , time: TimePeriod
                                                    , productionAgent: Self);

                DebugMessage("Created WorkItem: " + workSchedule.Name + " | Due:" + lastDue + " | Status: " + n.Status);
                lastDue = lastDue - workSchedule.Duration;
                firstItemToBuild = false;
                workItems.Add(n);
                // ToDO; 
                //Statistics.CreateSimulationWorkSchedule(n, RequestItem.OrderId.ToString(), RequestItem.IsHeadDemand);
            }
        }

        internal void SetWorkItemReady()
        {
            var workItems = Get<List<WorkItem>>(WORK_ITEMS);
            var hubAgents = Get<Dictionary<IActorRef, string>>(HUB_AGENTS);
            // check if there is something Ready or in Process Then just wait for their Ready Call
            if (workItems.Count() == 0 || workItems.Any(x => x.Status == ElementStatus.Ready || x.Status == ElementStatus.Processed))
            {
                DebugMessage("Requirement Missmatch!");
                return;
            }
                

            // get next ready WorkItem
            // TODO Return Queing Status ? or Move method to Machine
            var nextItem = workItems.Where(x => x.Status == ElementStatus.InQueue || x.Status == ElementStatus.Created)
                                    .OrderBy(x => x.WorkSchedule.HierarchyNumber)
                                    .FirstOrDefault();
            if (nextItem == null)
            {
                DebugMessage("Cannot start next.");
                return;
            }
            nextItem = nextItem.SetReady;

            if (hubAgents.Count == 0) return;
            var hubAgent = hubAgents.Single(x => x.Value == nextItem.WorkSchedule.MachineGroup.Name);

            SendWorkItemStatusMsg(hubAgent, nextItem);
        }

        private void SendWorkItemStatusMsg(KeyValuePair<IActorRef, string> hubAgent, WorkItem nextItem)
        {
            DebugMessage("Set next WorkItem Ready from Status " + nextItem.Status + " Time " + TimePeriod);


            // create StatusMsg
            var message = new ItemStatus(
                itemId: nextItem.Key,
                currentPriority: nextItem.Priority(TimePeriod),
                status: ElementStatus.Ready
            );

            // tell Item in Queue to set it ready.
            Send(instruction: Hub.Instruction.SetWorkItemStatus.Create(message, hubAgent.Key));
            nextItem = null;
            // ,waitFor: 1); // Start Production during the next time period
        }

        protected override void Finish()
        {
            var requestItem = this.Get<RequestItem>(REQUEST_ITEM);
            var workItems = this.Get<List<RequestItem>>(WORK_ITEMS);
            // Correct?
            if (requestItem.Provided && Context.GetChildren().Count() == 0 && workItems.All(x => x.Provided == true))
            {
                base.Finish();
            }
        }
    }
}
