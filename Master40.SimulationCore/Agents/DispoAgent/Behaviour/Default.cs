using System;
using System.Linq;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.ContractAgent;
using Master40.SimulationCore.Agents.DirectoryAgent;
using static Master40.SimulationCore.Agents.Guardian.Instruction;
using Master40.SimulationCore.Agents.ProductionAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Agents.SupervisorAgent;
using Master40.SimulationCore.Helper;
using static FArticles;
using static FAgentInformations;
using static FStockReservations;
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

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case Dispo.Instruction.RequestArticle r: RequestArticle(agent, r.GetObjectFromMessage); break; // Message From Contract or Production Agent
                case BasicInstruction.ResponseFromDirectory r: ResponseFromDirectory(agent, r.GetObjectFromMessage); break; // return with Storage from Directory
                case Dispo.Instruction.ResponseFromStock r: ResponseFromStock(agent, r.GetObjectFromMessage); break; // Message from Storage with Reservation
                case Dispo.Instruction.ResponseFromSystemForBom r: ResponseFromSystemForBom(agent, r.GetObjectFromMessage); break;
                case Dispo.Instruction.WithdrawMaterialsFromStock r: WithdrawMaterial((Dispo)agent); break;
                case BasicInstruction.ProvideArticle r: RequestProvided(agent, r.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        internal void RequestArticle(Agent agent, FArticle requestItem)
        {
            // Save Request Item.
            _fArticle = requestItem;
            // get related Storage Agent
            agent.Send(Directory.Instruction
                                .RequestAgent
                                .Create(descriminator: (string)requestItem.Article.Name
                                    , target: agent.ActorPaths.StorageDirectory.Ref));
        }

        internal void ResponseFromDirectory(Agent agent, FAgentInformation hubInfo)
        {
            agent.DebugMessage("Aquired stock Agent: " + hubInfo.Ref.Path.Name + " from " + agent.Sender.Path.Name);

            _fArticle = _fArticle.UpdateStorageAgent(hubInfo.Ref);
            // Create Request to Storage Agent 
            agent.Send(Storage.Instruction.RequestArticle.Create(_fArticle, hubInfo.Ref));
        }

        internal void ResponseFromStock(Agent agent, FStockReservation reservation)
        {
            _fArticle = _fArticle.UpdateStockExchangeId(reservation.TrackingId);
           
            _quantityToProduce = _fArticle.Quantity - reservation.Quantity;

            agent.DebugMessage(reservation.Quantity + " " 
                                + _fArticle.Article.Name + " are reserved and " 
                                + _quantityToProduce + " " + _fArticle.Article.Name + " need to be produced!");

            if (reservation.IsInStock == true)
            {
                RequestProvided(agent, _fArticle);
                agent.TryToFinish();
                return;
            }

            // else Create Production Agents if ToBuild
            if (_fArticle.Article.ToBuild)
            {
                agent.Send(RequestArticleBom.Create(_fArticle.Article.Id, agent.ActorPaths.SystemAgent.Ref));

                // and request the Article from  stock at Due Time
                if (_fArticle.IsHeadDemand)
                {
                    var nextRequestAt = _fArticle.DueTime - agent.CurrentTime;
                    agent.DebugMessage("Ask storage for Article at " + nextRequestAt + " article: " + _fArticle.Key);

                    agent.Send(instruction: ProvideArticleAtDue.Create(_fArticle.Key, _fArticle.StorageAgent)
                                 , waitFor: nextRequestAt);
                }
            }
            // Not in Stock and Not ToBuild Agent has to Wait for Stock To Provide Materials
        }

        internal void ResponseFromSystemForBom(Agent agent, M_Article article)
        {
            // Update
            long dueTime = _fArticle.DueTime;

            if (article.Operations != null)
                dueTime = _fArticle.DueTime - article.Operations.Sum(x => x.Duration + x.AverageTransitionDuration);
            // TODO: Object that handles the diffrent operations- current asumption is all operations are handled as a sequence (no alternative/parallel plans) 

            _fArticle = _fArticle.UpdateCustomerOrderAndDue(_fArticle.CustomerOrderId, dueTime, _fArticle.StorageAgent)
                                             .UpdateArticle(article);

            // Creates a Production Agent for each element that has to be produced
            for (int i = 0; i < _quantityToProduce; i++)
            {
                var agentSetup = AgentSetup.Create(agent, ProductionAgent.Behaviour.Factory.Get(SimulationType.None));
                var instruction = CreateChild.Create(agentSetup, agent.Guardian);
                agent.Send(instruction);
            }
        }

        internal void RequestProvided(Agent agent, FArticle fArticle)
        {
            // TODO: Might be problematic due to inconsistent _fArticle != Strorage._fArticle
            agent.DebugMessage("Request Provided from " + agent.Sender);

            _fArticle = fArticle.SetProvided.UpdateFinishedAt(agent.CurrentTime);
            agent.Send(BasicInstruction.ProvideArticle.Create(_fArticle, agent.VirtualParent, false));
        }

        internal void WithdrawMaterial(Dispo agent)
        {
            agent.Send(WithdrawlMaterial
                              .Create(message: _fArticle.StockExchangeId
                                     , target: _fArticle.StorageAgent));
            agent.TryToFinish();
        }
    }
}
