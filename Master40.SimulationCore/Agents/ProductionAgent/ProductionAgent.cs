using Akka.Actor;
using Master40.DB.Models;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents
{
    public partial class ProductionAgent : Agent
    {
        /*
        private RequestItem _requestItem;
        private List<WorkItem> _workItems { get; set; }
        private List<RequestItem> _requestItems { get; set; }
        private Dictionary<IActorRef, string> _hubAgents;
        private ElementStatus _status;
        private WorkItem _nextItem {get; set;}
        */
        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug, RequestItem item)
        {
            return Akka.Actor.Props.Create(() => new ProductionAgent(actorPaths, time, debug, item));
        }

        public ProductionAgent(ActorPaths actorPaths, long time, bool debug, RequestItem item) : base(actorPaths, time, debug)
        {
            _requestItem = item;
            //RequestArtikles = new List<RequestItem>();
            _status = ElementStatus.Created;
            _hubAgents = new Dictionary<IActorRef, string>();
            _requestItems = new List<RequestItem>();
            DebugMessage("Woke up. My dueTime is :" + item.DueTime);
        }

        protected override void DoNot(object o)
        {
            switch (o)
            {
                case BasicInstruction.Initialize i: StartProductionAgent(); break;
                case Instruction.SetHubAgent s: SetHubAgent(s); break;
                case Instruction.FinishWorkItem fw: FinishWorkItem(fw); break;
                case Instruction.ProductionStarted ps: ProductionStarted(ps.GetObjectfromMessage); break;
                case Instruction.ProvideRequest pr: ProvideRequest(pr.GetObjectFromMessage); break;
                case Instruction.Finished f: Finished(f.GetObjectFromMessage); break;
                default: throw new Exception("Invalid Message Object.");
            }
        }

        /*
        private void ProvideRequest(ItemStatus itemStatus)
        {
            var item = _requestItems.Single(x => x.Key == itemStatus.ItemId);
            DebugMessage("Item to Remove from requestItems: " + item.Article.Name + " --> left " + (_workItems.Count() - 1));
            _requestItems.Remove(item);
            if (_workItems.Any() && _requestItems.Count() == 0)
            {
                SetWorkItemReady();
            }
            if (_status == ElementStatus.Finished)
            {
                base.Finish();
            }
        }
        */ 

        private void StartProductionAgent()
        {
            var firstToEnqueue = false;
            // check for Children
            if (!_requestItem.Article.ArticleBoms.Any())
            {
                DebugMessage("Last leave in Bom");
                firstToEnqueue = true;
            }

            // if item hase Workschedules Request ComClient for them
            if (_requestItem.Article.WorkSchedules != null)
            {
                // Ask the Directory Agent for Service
                RequestHubAgentFor(workSchedules: _requestItem.Article.WorkSchedules);
                // And Create workItems
                CreateWorkItemsFromRequestItem(firstItemToBuild: firstToEnqueue);
            }

            // Create Dispo Agents for Childs.
            foreach (var articleBom in _requestItem.Article.ArticleBoms)
            {
                // create Dispo Agents for to Provide Required Articles
                
                var dispoAgent = UntypedActor.Context.ActorOf(props: DispoAgent.Props(ActorPaths, TimePeriod, DebugThis)
                        , name: ("Dispo(" + articleBom.Name + "_ChildOf(" + this.Name + ")_No_" + Guid.NewGuid()).ToActorName() );


                var item = new RequestItem(
                    key: Guid.NewGuid()
                    , article: articleBom.ArticleChild
                    , stockExchangeId: Guid.Empty
                    , storageAgent: null
                    , quantity: Convert.ToInt32(articleBom.Quantity)
                    , dueTime: _requestItem.DueTime
                    , requester: dispoAgent
                    , providerList: new List<IActorRef>()
                    , orderId: _requestItem.OrderId
                    , providable: 0
                    , provided: false
                    , isHeadDemand: false);

                _requestItems.Add(item);
                // Send Request
                CreateAndEnqueue(DispoAgent.Instruction.RequestArticle.Create(item, dispoAgent));
            }
        }

        internal void Finished(ItemStatus status)
        {
            // // any Not Finished do noting
            // if (ChildAgents.Any(x => x.Status != Status.Finished))
            //     return;
            
            // Return from Production as WorkItemStatus
            if (status != null)
            {
                var workItem = _workItems.First(x => x.Key == status.ItemId);
                _workItems.Replace(workItem.UpdateStatus(status.Status));
            }

            // TODO Anything ?
            if (_requestItem.Article.WorkSchedules != null && _workItems.All(x => x.Status == ElementStatus.Finished))
            {
                _status = ElementStatus.Finished;
                CreateAndEnqueue(instruction: StorageAgent.Instruction.ResponseFromProduction.Create(_requestItem, _requestItem.StorageAgent));

                DebugMessage("All Workschedules have been Finished");
                base.Finish();
                return;
            }
            // else
            DebugMessage("Im Ready To get Enqued");
            _status = (Status == ElementStatus.Processed) ? ElementStatus.Processed : ElementStatus.Ready;
            _workItems.ForEach(item => item = item.UpdateMaterialsProvided(true));
            if (_hubAgents.Count > 0)
            {
                SetWorkItemReady();
            }
        }
    

        private void RequestHubAgentFor(ICollection<WorkSchedule> workSchedules)
        {
            // Request Comunication Agent for my Workschedules
            var machineGroups = workSchedules.Select(x => x.MachineGroup.Name).Distinct().ToList();
            foreach (var machineGroupName in machineGroups)
            {   
                CreateAndEnqueue(DirectoryAgent.Instruction
                                               .RequestRessourceAgent
                                               .Create(descriminator: machineGroupName
                                                       ,target: ActorPaths.HubDirectory.Ref));
            }
        }

        private void SetHubAgent(Instruction.SetHubAgent instruction)
        {
            // Enque my Element at Comunication Agent
            var hub = instruction.GetObjectFromMessage;
            if (hub == null)
            {
                throw new InvalidCastException(" Could not Cast Comunication agent on InstructionSet.");
            }
            DebugMessage("Recived Agent from Directory: " + Sender.Path.Name);

            // add agent to current Scope.
            _hubAgents.Add(hub.Ref, hub.RequiredFor);
            // foreach fitting WorkSchedule
            foreach (var workItem in _workItems.Where(x => x.WorkSchedule.MachineGroup.Name == hub.RequiredFor))
            {
                CreateAndEnqueue(instruction: HubAgent.Instruction.EnqueueWorkItem.Create(workItem, hub.Ref));
            }
        }

        private void CreateWorkItemsFromRequestItem(bool firstItemToBuild)
        {
            _workItems = new List<WorkItem>();
            var lastDue = _requestItem.DueTime;
            foreach (var workSchedule in _requestItem.Article.WorkSchedules.OrderBy(x => x.HierarchyNumber))
            {
                var n = workSchedule.ToWorkItem(dueTime: lastDue
                                                    , status: firstItemToBuild ? ElementStatus.Ready : ElementStatus.Created
                                                    , time: TimePeriod
                                                    , productionAgent: Self);

                DebugMessage("Created WorkItem: " + workSchedule.Name + " | Due:" + lastDue + " | Status: " + n.Status);
                lastDue = lastDue - workSchedule.Duration;
                firstItemToBuild = false;
                _workItems.Add(n);
                // ToDO; 
                //Statistics.CreateSimulationWorkSchedule(n, RequestItem.OrderId.ToString(), RequestItem.IsHeadDemand);
            }
        }

        internal void SetWorkItemReady()
        {
            // check if there is something Ready or in Process Then just wait for their Ready Call
            if (_workItems.Count() == 0 || _workItems.Any(x => x.Status == ElementStatus.Ready || x.Status == ElementStatus.Processed))
            {
                DebugMessage("Requirement Missmatch");
                return;
            }
                

            // get next ready WorkItem
            // TODO Return Queing Status ? or Move method to Machine
            _nextItem = _workItems.Where(x => x.Status == ElementStatus.InQueue || x.Status == ElementStatus.Created)
                                    .OrderBy(x => x.WorkSchedule.HierarchyNumber)
                                    .FirstOrDefault();
            if (_nextItem == null)
            {
                DebugMessage("Cannot start next.");
                return;
            }
            _nextItem = _nextItem.SetReady;

            if (_hubAgents.Count == 0) return;
            var hubAgent = _hubAgents.Single(x => x.Value == _nextItem.WorkSchedule.MachineGroup.Name);

            SendWorkItemStatusMsg(hubAgent);
        }

        private void SendWorkItemStatusMsg(KeyValuePair<IActorRef, string> hubAgent) {
            DebugMessage("Set next WorkItem Ready from Status " + _nextItem.Status + " Time " + TimePeriod);


            // create StatusMsg
            var message = new ItemStatus(
                itemId: _nextItem.Key,
                currentPriority: _nextItem.Priority(TimePeriod),
                status: ElementStatus.Ready
            );

            // tell Item in Queue to set it ready.
            CreateAndEnqueue(instruction: HubAgent.Instruction.SetWorkItemStatus.Create(message, hubAgent.Key));
            _nextItem = null;
            // ,waitFor: 1); // Start Production during the next time period
        }

        private void FinishWorkItem(Instruction.FinishWorkItem instruction)
        {
            if (instruction.GetObjectFromMessage == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }
            DebugMessage("Machine called finished with: " + instruction.GetObjectFromMessage.WorkSchedule.Name + " !");

            // Shortcut:
            //CreateAndEnqueue
            Finished(new ItemStatus(instruction.GetObjectFromMessage.Key
                                    , instruction.GetObjectFromMessage.Status
                                    , instruction.GetObjectFromMessage.ItemPriority ));
        }

        protected override void Finish()
        {
            if (_requestItem.Provided && Context.GetChildren().Count() == 0 && _workItems.All(x => x.Status == ElementStatus.Finished))
            {
                base.Finish();
            }
        }

        private void ProductionStarted(WorkItem workItem)
        {
            if (_status != workItem.Status)
            {
                _status = ElementStatus.Processed;
                
                foreach (var agent in UntypedActor.Context.GetChildren())
                {
                    CreateAndEnqueue(DispoAgent.Instruction
                                               .WithdrawMaterialsFromStock
                                               .Create(message: "Production Start"
                                                      , target: agent));
                }
            }

        }

    }
}
