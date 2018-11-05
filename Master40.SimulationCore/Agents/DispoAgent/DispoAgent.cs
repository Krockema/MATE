using Akka.Actor;
using Akka.Event;
using Master40.DB.Models;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Agents;
using Master40.SimulationImmutables;
using System;
using System.Linq;


namespace Master40.SimulationCore.Agents
{
    /// <summary>
    /// --------- General sequence
    /// 
    /// Contract -> Request Article     ->  Dispo
    ///                                     Dispo -> Request Stock for Article from -> Directory
    /// Directory -> Response with Stock -> Dispo
    ///                                     Dispo -> Request Article from Stock -> Stock
    /// Stock -> Response from stock    ->  Dispo                                    
    /// </summary>

    public partial class DispoAgent : Agent
    {
        private IActorRef _stockAgent { get; set; }
        private int _quantityToProduce { get; set; }
        private RequestItem _requestItem { get; set; }
        

        // public Constructor
        public static Props Props(ActorPaths actorPaths, long time, bool debug)
        {
            return Akka.Actor.Props.Create(() => new DispoAgent(actorPaths, time, debug));
        }
        public DispoAgent(ActorPaths actorPaths, long time, bool debug) : base(actorPaths, time, debug)
        {
        }

        protected void DoNot(object o)
        {
            switch (o)
            {
                case BasicInstruction.ResponseFromHub rfd: ResponseFromHub(rfd.GetObjectFromMessage); break;
                case Instruction.ResponseFromSystemForBom a: ResponseFromSystemForBom(a.GetObjectFromMessage); break;
                case Instruction.RequestProvided rp: RequestProvided(rp.GetObjectFromMessage); break;
                case Instruction.WithdrawMaterialsFromStock wm: WithdarwMaterial(); break;
                default: throw new Exception("Invalid Message Object.");
            }
        }

        private void ResponseFromHub(HubInformation hub)
        {
            if ((_stockAgent = hub.Ref) == null)
            {
                throw new ArgumentNullException($"No storage agent found for {1}", _requestItem.Article.Name);
            }
            // debug
            DebugMessage("Aquired stock Agent: " + _stockAgent.Path.Name + " from " + Sender.Path.Name);

            // Create Request 
            CreateAndEnqueue(StorageAgent.Instruction.RequestArticle.Create(_requestItem, _stockAgent));
        }

        private void ResponseFromSystemForBom(Article article)
        {
            // Update 
            long dueTime = _requestItem.DueTime;
            if (_requestItem.Article.WorkSchedules != null)
                dueTime = _requestItem.DueTime - _requestItem.Article.WorkSchedules.Sum(x => x.Duration); //- Calculations.GetTransitionTimeForWorkSchedules(item.Article.WorkSchedules);
            

            RequestItem item = _requestItem.UpdateOrderAndDue(_requestItem.OrderId, dueTime, _stockAgent)
                                           .UpdateArticle(article);
           
            // Creates a Production Agent for each element that has to be produced
            for (int i = 0; i < _quantityToProduce; i++)
            {
                var agent = UntypedActor.Context.ActorOf(props: ProductionAgent.Props(ActorPaths, TimePeriod, DebugThis, item),
                                name: ("Production(" + item.Article.Name + "_Nr." + i + ")").ToActorName());
                CreateAndEnqueue(BasicInstruction.Initialize.Create(agent));
            }
        }

        private void RequestProvided(RequestItem o)
        {
            DebugMessage("Request Provided from " + Sender);
            if (_requestItem.IsHeadDemand)
            {
                CreateAndEnqueue(ContractAgent.Instruction.Finish.Create(_requestItem, UntypedActor.Context.Parent));
            }
            else
            {
                CreateAndEnqueue(ProductionAgent.Instruction
                                                .ProvideRequest.Create(message: new ItemStatus(_requestItem.Key, ElementStatus.Finished, 2)
                                                                ,target: this.Context.Parent));
            }
            _requestItem = _requestItem.SetProvided;
            Finish();
        }

        internal void ShutdownAgent()
        {
            Finish();
        }

        protected override void Finish()
        {
            var children = this.Context.GetChildren();
            if (_requestItem.Provided == true && children.Count() == 0)
            {
                base.Finish();
            }
        }

        private void WithdarwMaterial()
        {
            CreateAndEnqueue(StorageAgent.Instruction
                                         .WithdrawlMaterial.Create(message: _requestItem.StockExchangeId
                                                                   ,target: _stockAgent));
        }
    }
}
