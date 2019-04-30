using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.DirectoryAgent;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.ProductionAgent
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
            DebugMessage("I'm Alive:" + Context.Self.Path);
            //this.Send(BasicInstruction.Initialize.Create(this.Context.Self, ProductionBehaviour.Get()));
        }
        protected override void OnChildAdd(IActorRef childRef)
        {
            var childItems = Get<Queue<FRequestItem>>(Properties.CHILD_WORKITEMS);
            var requesteditems = Get<List<FRequestItem>>(Properties.REQUESTED_ITEMS);
            var requestItem = childItems.Dequeue();
            requesteditems.Add(requestItem);
            this.Send(Dispo.Instruction.RequestArticle.Create(requestItem, childRef));
            this.DebugMessage("Production<" + requestItem.Article.Name + "(OrderId: " + requestItem.CustomerOrderId + ") >");
        }


        internal void Finished(FItemStatus status)
        {
            // // any Not Finished do noting
            // if (ChildAgents.Any(x => x.Status != Status.Finished))
            //     return;
            var workItems = Get<List<FWorkItem>>(Properties.WORK_ITEMS);
            var requestItem = Get<FRequestItem>(Properties.REQUEST_ITEM);
            var hubAgents = Get<Dictionary<IActorRef, string>>(Properties.HUB_AGENTS);
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
    

        internal void RequestHubAgentFor(ICollection<M_Operation> workSchedules)
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

        internal void CreateWorkItemsFromRequestItem(bool firstItemToBuild, FRequestItem requestItem)
        {

            var workItems =  Get<List<FWorkItem>>(Properties.WORK_ITEMS);
            var lastDue = requestItem.DueTime;
            foreach (var workSchedule in Enumerable.OrderBy<M_Operation, int>(requestItem.Article.WorkSchedules, x => x.HierarchyNumber))
            {
                var n = workSchedule.ToWorkItem(dueTime: lastDue
                                                    //, status: firstItemToBuild ? ElementStatus.Ready : ElementStatus.Created
                                                    , status: ElementStatus.Created
                                                    , time: TimePeriod
                                                    , productionAgent: Self);

                DebugMessage("Created WorkItem: " + workSchedule.Name + " | Due:" + lastDue + " | Status: " + n.Status);
                lastDue = lastDue - workSchedule.Duration;
                firstItemToBuild = false;
                workItems.Add(n);
                // ToDO; 
                var pub = new CreateSimulationWork(n, requestItem.CustomerOrderId.ToString(), requestItem.IsHeadDemand, requestItem.Article.ArticleType.Name);
                this.Context.System.EventStream.Publish(pub);

                //Statistics.CreateSimulationWorkSchedule(n, RequestItem.OrderId.ToString(), RequestItem.IsHeadDemand);
            }
        }

        internal void SetWorkItemReady()
        {
            var workItems = Get<List<FWorkItem>>(Properties.WORK_ITEMS);
            var hubAgents = Get<Dictionary<IActorRef, string>>(Properties.HUB_AGENTS);
            // check if there is something Ready or in Process Then just wait for their Ready Call
            if (workItems.Count() == 0 || workItems.Any(x => x.Status == ElementStatus.Ready || x.Status == ElementStatus.Processed))
            {
                DebugMessage("Requirement Missmatch!");
                return;
            }
                

            // get next ready WorkItem
            // TODO Return Queing Status ? or Move method to Machine
            var nextItem = workItems.Where(x => x.Status == ElementStatus.InQueue || x.Status == ElementStatus.Created)
                                    .OrderBy<FWorkItem, int>(x => x.WorkSchedule.HierarchyNumber)
                                    .FirstOrDefault();
            if (nextItem == null)
            {
                DebugMessage("Cannot start next.");
                return;
            }
            nextItem = nextItem.SetReady;

            if (hubAgents.Count == 0) return;
            var hubAgent = hubAgents.Single((KeyValuePair<IActorRef, string> x) => x.Value == nextItem.WorkSchedule.MachineGroup.Name);

            SendWorkItemStatusMsg(hubAgent, nextItem);
        }

        private void SendWorkItemStatusMsg(KeyValuePair<IActorRef, string> hubAgent, FWorkItem nextItem)
        {
            DebugMessage("Set next WorkItem Ready from Status " + nextItem.Status + " Time " + TimePeriod);


            // create StatusMsg
            var message = new FItemStatus(
                itemId: nextItem.Key,
                currentPriority: nextItem.Priority(TimePeriod),
                status: ElementStatus.Ready
            );

            // tell Item in Queue to set it ready.
            Send(instruction: Hub.Instruction.SetWorkItemStatus.Create(message, hubAgent.Key));
            // ,waitFor: 1); // Start Production during the next time period
            Set(Properties.NEXT_WORK_ITEM, null);
        }

        protected override void Finish()
        {
            var requestItem = this.Get<FRequestItem>(Properties.REQUEST_ITEM);
            var workItems = this.Get<List<FRequestItem>>(Properties.WORK_ITEMS);
            // Correct?
            if (requestItem.Provided && VirtualChilds.Count() == 0 && workItems.All(x => x.Provided == true))
            {
                base.Finish();
            }
        }
    }
}
