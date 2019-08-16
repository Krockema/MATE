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
                        : base(null, simulationType) { }


        internal FArticle _fArticle { get; set; }
        internal int _quantityToProduce { get; set; }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Dispo.Instruction.RequestArticle r: RequestArticle(r.GetObjectFromMessage); break; // Message From Contract or Production Agent
                case BasicInstruction.ResponseFromDirectory r: ResponseFromDirectory(r.GetObjectFromMessage); break; // return with Storage from Directory
                case Dispo.Instruction.ResponseFromStock r: ResponseFromStock(r.GetObjectFromMessage); break; // Message from Storage with Reservation
                case Dispo.Instruction.ResponseFromSystemForBom r: ResponseFromSystemForBom(r.GetObjectFromMessage); break;
                case Dispo.Instruction.WithdrawMaterialsFromStock r: WithdrawMaterial(); break;
                case BasicInstruction.ProvideArticle r: RequestProvided(r.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        internal void RequestArticle(FArticle requestItem)
        {
            // Save Request Item.
            _fArticle = requestItem;
            // get related Storage Agent
            Agent.Send(Directory.Instruction
                                .RequestAgent
                                .Create(discriminator: (string)requestItem.Article.Name
                                    , target: Agent.ActorPaths.StorageDirectory.Ref));
        }

        internal void ResponseFromDirectory(FAgentInformation hubInfo)
        {
            Agent.DebugMessage("Acquired stock Agent: " + hubInfo.Ref.Path.Name + " from " + Agent.Sender.Path.Name);

            _fArticle = _fArticle.UpdateStorageAgent(hubInfo.Ref);
            // Create Request to Storage Agent 
            Agent.Send(Storage.Instruction.RequestArticle.Create(_fArticle, hubInfo.Ref));
        }

        internal void ResponseFromStock(FStockReservation reservation)
        {
            _fArticle = _fArticle.UpdateStockExchangeId(reservation.TrackingId);
           
            _quantityToProduce = _fArticle.Quantity - reservation.Quantity;

            Agent.DebugMessage(reservation.Quantity + " " 
                                + _fArticle.Article.Name + " are reserved and " 
                                + _quantityToProduce + " " + _fArticle.Article.Name + " need to be produced!");

            if (reservation.IsInStock == true)
            {
                RequestProvided(_fArticle);
                Agent.TryToFinish();
                return;
            }

            // else Create Production Agents if ToBuild
            if (_fArticle.Article.ToBuild)
            {
                Agent.Send(RequestArticleBom.Create(_fArticle.Article.Id, Agent.ActorPaths.SystemAgent.Ref));

                // and request the Article from  stock at Due Time
                if (_fArticle.IsHeadDemand)
                {
                    var nextRequestAt = _fArticle.DueTime - Agent.CurrentTime;
                    Agent.DebugMessage("Ask storage for Article at " + nextRequestAt + " article: " + _fArticle.Key);

                    Agent.Send(instruction: ProvideArticleAtDue.Create(_fArticle.Key, _fArticle.StorageAgent)
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
                dueTime = _fArticle.DueTime - article.Operations.Sum(x => x.Duration + x.AverageTransitionDuration);
            // TODO: Object that handles the different operations- current assumption is all operations are handled as a sequence (no alternative/parallel plans) 

            _fArticle = _fArticle.UpdateCustomerOrderAndDue(_fArticle.CustomerOrderId, dueTime, _fArticle.StorageAgent)
                                             .UpdateArticle(article);

            // Creates a Production Agent for each element that has to be produced
            for (var i = 0; i < _quantityToProduce; i++)
            {
                var agentSetup = AgentSetup.Create(Agent, ProductionAgent.Behaviour.Factory.Get(SimulationType.None));
                var instruction = CreateChild.Create(agentSetup, Agent.Guardian);
                Agent.Send(instruction);
            }
        }

        internal void RequestProvided(FArticle fArticle)
        {
            // TODO: Might be problematic due to inconsistent _fArticle != Storage._fArticle
            Agent.DebugMessage("Request Provided from " + Agent.Sender);

            _fArticle = fArticle.SetProvided.UpdateFinishedAt(Agent.CurrentTime);
            Agent.Send(BasicInstruction.ProvideArticle.Create(_fArticle, Agent.VirtualParent, false));
        }

        internal void WithdrawMaterial()
        {
            Agent.Send(WithdrawlMaterial
                              .Create(message: _fArticle.StockExchangeId
                                     , target: _fArticle.StorageAgent));
            Agent.TryToFinish();
        }
    }
}
