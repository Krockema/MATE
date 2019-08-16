using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.DirectoryAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Helper;
using System.Linq;
using static FAgentInformations;
using static FArticles;
using static FStockReservations;
using static Master40.SimulationCore.Agents.Guardian.Instruction;
using static Master40.SimulationCore.Agents.StorageAgent.Storage.Instruction;
using static Master40.SimulationCore.Agents.SupervisorAgent.Supervisor.Instruction;

namespace Master40.SimulationCore.Agents.DispoAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour
    {
        internal Default(SimulationType simulationType = SimulationType.None) 
                        : base(childMaker: null, obj: simulationType) { }


        internal FArticle _fArticle { get; set; }
        internal int _quantityToProduce { get; set; }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Dispo.Instruction.RequestArticle r: RequestArticle(requestItem: r.GetObjectFromMessage); break; // Message From Contract or Production Agent
                case BasicInstruction.ResponseFromDirectory r: ResponseFromDirectory(hubInfo: r.GetObjectFromMessage); break; // return with Storage from Directory
                case Dispo.Instruction.ResponseFromStock r: ResponseFromStock(reservation: r.GetObjectFromMessage); break; // Message from Storage with Reservation
                case BasicInstruction.JobForwardEnd msg: PushForwardTimeToParent(earliestStartForForwardScheduling: msg.GetObjectFromMessage); break; // push Calculated Forward Calculated
                case Dispo.Instruction.ResponseFromSystemForBom r: ResponseFromSystemForBom(article: r.GetObjectFromMessage); break;
                case Dispo.Instruction.WithdrawMaterialsFromStock r: WithdrawMaterial(); break;
                case BasicInstruction.ProvideArticle r: RequestProvided(fArticle: r.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private void PushForwardTimeToParent(long earliestStartForForwardScheduling)
        {
            if (_fArticle.IsHeadDemand) return;
            
            var msg = BasicInstruction.JobForwardEnd.Create(message: earliestStartForForwardScheduling, target: Agent.VirtualParent);
            Agent.Send(instruction: msg);
        }

        internal void RequestArticle(FArticle requestItem)
        {
            // Save Request Item.
            _fArticle = requestItem;
            // get related Storage Agent
            Agent.Send(instruction: Directory.Instruction
                                .RequestAgent
                                .Create(discriminator: (string)requestItem.Article.Name
                                    , target: Agent.ActorPaths.StorageDirectory.Ref));
        }

        internal void ResponseFromDirectory(FAgentInformation hubInfo)
        {
            Agent.DebugMessage(msg: "Acquired stock Agent: " + hubInfo.Ref.Path.Name + " from " + Agent.Sender.Path.Name);

            _fArticle = _fArticle.UpdateStorageAgent(s: hubInfo.Ref);
            // Create Request to Storage Agent 
            Agent.Send(instruction: Storage.Instruction.RequestArticle.Create(message: _fArticle, target: hubInfo.Ref));
        }

        internal void ResponseFromStock(FStockReservation reservation)
        {
            _fArticle = _fArticle.UpdateStockExchangeId(i: reservation.TrackingId);
           
            _quantityToProduce = _fArticle.Quantity - reservation.Quantity;

            Agent.DebugMessage(msg: reservation.Quantity + " " 
                                + _fArticle.Article.Name + " are reserved and " 
                                + _quantityToProduce + " " + _fArticle.Article.Name + " need to be produced!");

            if (reservation.IsInStock == true)
            {
                RequestProvided(fArticle: _fArticle);
                Agent.TryToFinish();
                return;
            }

            // else Create Production Agents if ToBuild
            if (_fArticle.Article.ToBuild)
            {
                Agent.Send(instruction: RequestArticleBom.Create(message: _fArticle.Article.Id, target: Agent.ActorPaths.SystemAgent.Ref));

                // and request the Article from  stock at Due Time
                if (_fArticle.IsHeadDemand)
                {
                    var nextRequestAt = _fArticle.DueTime - Agent.CurrentTime;
                    Agent.DebugMessage(msg: "Ask storage for Article at " + nextRequestAt + " article: " + _fArticle.Key);

                    Agent.Send(instruction: ProvideArticleAtDue.Create(message: _fArticle.Key, target: _fArticle.StorageAgent)
                                 , waitFor: nextRequestAt);
                }
            }
            // Not in Stock and Not ToBuild Agent has to Wait for Stock To Provide Materials
        }

        internal void ResponseFromSystemForBom(M_Article article)
        {
            // Update
            var dueTime = _fArticle.DueTime;

            if (article.Operations != null)
                dueTime = _fArticle.DueTime - article.Operations.Sum(selector: x => x.Duration + x.AverageTransitionDuration);
            // TODO: Object that handles the different operations- current assumption is all operations are handled as a sequence (no alternative/parallel plans) 

            _fArticle = _fArticle.UpdateCustomerOrderAndDue(id: _fArticle.CustomerOrderId, due: dueTime, storage: _fArticle.StorageAgent)
                                             .UpdateArticle(article: article);

            // Creates a Production Agent for each element that has to be produced
            for (var i = 0; i < _quantityToProduce; i++)
            {
                var agentSetup = AgentSetup.Create(agent: Agent, behaviour: ProductionAgent.Behaviour.Factory.Get(simType: SimulationType.None));
                var instruction = CreateChild.Create(setup: agentSetup, target: ((Dispo)Agent).Guardian, source: Agent.Context.Self);
                Agent.Send(instruction: instruction);
            }
        }

        internal void RequestProvided(FArticle fArticle)
        {
            // TODO: Might be problematic due to inconsistent _fArticle != Storage._fArticle
            Agent.DebugMessage(msg: "Request Provided from " + Agent.Sender);

            _fArticle = fArticle.SetProvided.UpdateFinishedAt(f: Agent.CurrentTime);
            Agent.Send(instruction: BasicInstruction.ProvideArticle.Create(message: _fArticle, target: Agent.VirtualParent, logThis: false));
        }

        internal void WithdrawMaterial()
        {
            Agent.Send(instruction: WithdrawlMaterial
                              .Create(message: _fArticle.StockExchangeId
                                     , target: _fArticle.StorageAgent));
            Agent.TryToFinish();
        }
    }
}
