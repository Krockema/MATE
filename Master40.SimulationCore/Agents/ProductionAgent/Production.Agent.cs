using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.DirectoryAgent;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Agents.HubAgent;
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

        internal void CreateWorkItemsFromRequestItem(bool firstItemToBuild, FRequestItem requestItem)
        {

            var workItems = Get<List<FWorkItem>>(Properties.WORK_ITEMS);
            var lastDue = requestItem.DueTime;
            foreach (var workSchedule in Enumerable.OrderBy(requestItem.Article.WorkSchedules, x => x.HierarchyNumber))
            {
                var n = workSchedule.ToWorkItem(dueTime: lastDue
                                                    , status: ElementStatus.Created
                                                    , time: TimePeriod
                                                    , productionAgent: Self);

                DebugMessage("Created WorkItem: " + workSchedule.Name + " | Due:" + lastDue + " | Status: " + n.Status + "workItemId " + n.Key);
                lastDue = lastDue - workSchedule.Duration;
                workItems.Add(n);
                // ToDO; 
                var pub = new CreateSimulationWork(n, requestItem.CustomerOrderId.ToString(), requestItem.IsHeadDemand, requestItem.Article.ArticleType.Name);
                this.Context.System.EventStream.Publish(pub);

                //Statistics.CreateSimulationWorkSchedule(n, RequestItem.OrderId.ToString(), RequestItem.IsHeadDemand);
            }
        }

        protected override void Finish()
        {
            var workItems = this.Get<List<FWorkItem>>(Properties.WORK_ITEMS);
            var requestItem = this.Get<FRequestItem>(Properties.REQUEST_ITEM);
            // Correct?
            if (VirtualChilds.Count() == 0 && workItems.All(x => x.Status == ElementStatus.Finished))
            {
                var keys = "";
                workItems.ForEach(x => keys = x.Key + ";");
                DebugMessage("Shutdown Agent: " + keys + " RequestItemId: " + requestItem.Key);
                this.Send(BasicInstruction.RemoveVirtualChild.Create(VirtualParent));
                base.Finish();
            }
        }
    }
}