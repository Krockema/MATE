using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.DirectoryAgent;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using static FAgentInformations;
using static FArticleProviders;
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
                        : base(childMaker: null, simulationType: simulationType) { }


        internal FArticle _fArticle { get; set; }
        internal int _quantityToProduce { get; set; }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Dispo.Instruction.RequestArticle r: RequestArticle(requestArticle: r.GetObjectFromMessage); break; // Message From Contract or Production Agent
                case BasicInstruction.ResponseFromDirectory r: ResponseFromDirectory(hubInfo: r.GetObjectFromMessage); break; // return with Storage from Directory
                case Dispo.Instruction.ResponseFromStock r: ResponseFromStock(reservation: r.GetObjectFromMessage); break; // Message from Storage with Reservation
                case BasicInstruction.JobForwardEnd msg: PushForwardTimeToParent(earliestStartForForwardScheduling: msg.GetObjectFromMessage); break; // push Calculated Forward Calculated
                case Dispo.Instruction.ResponseFromSystemForBom r: ResponseFromSystemForBom(article: r.GetObjectFromMessage); break;
                case Dispo.Instruction.WithdrawArticleFromStock r: WithdrawArticleFromStock(); break; // Withdraw initiated by ResourceAgent for Production
                case BasicInstruction.ProvideArticle r: ProvideRequest(fArticleProvider: r.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }
        
        internal void RequestArticle(FArticle requestArticle)
        {
            // Save Request Item.
            _fArticle = requestArticle;
            Agent.DebugMessage($"{_fArticle.Article.Name} {_fArticle.Key} is Requested to Produce.");
            // get related Storage Agent
            Agent.Send(instruction: Directory.Instruction
                                .RequestAgent
                                .Create(discriminator: requestArticle.Article.Name
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

            if (reservation.IsInStock && !_fArticle.Article.ToBuild)
            {
                Agent.DebugMessage(msg: $"Start forward scheduling for article: {_fArticle.Article.Name} {_fArticle.Key} at: {Agent.CurrentTime}");
                PushForwardTimeToParent(earliestStartForForwardScheduling: Agent.CurrentTime);
            }


            if (reservation.IsInStock && !_fArticle.IsHeadDemand)
            {
                ProvideRequest(new FArticleProvider(articleKey: _fArticle.Key
                                                  ,articleName: _fArticle.Article.Name
                                                  , stockExchangeId: reservation.TrackingId
                                                     ,provider: new List<Guid>(new[] { reservation.TrackingId })));
            }

            // else create Production Agents if ToBuild
            if (_fArticle.Article.ToBuild)
            {
                Agent.Send(instruction: RequestArticleBom.Create(message: _fArticle.Article.Id, target: Agent.ActorPaths.SystemAgent.Ref));

                // and request the Article from  stock at Due Time
                if (_fArticle.IsHeadDemand)
                {
                    var nextRequestAt = _fArticle.DueTime - Agent.CurrentTime;
                    Agent.DebugMessage(msg: $"Ask storage for Article {_fArticle.Key} in + {nextRequestAt}");

                    Agent.Send(instruction: ProvideArticleAtDue.Create(message: _fArticle.Key, target: _fArticle.StorageAgent)
                                 , waitFor: nextRequestAt);
                }
            }
            // Not in Stock and Not ToBuild Agent has to Wait for stock to provide materials
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
                var instruction = CreateChild.Create(setup: agentSetup, target: ((IAgent)Agent).Guardian, source: Agent.Context.Self);
                Agent.Send(instruction: instruction);
            }
        }

        private void PushForwardTimeToParent(long earliestStartForForwardScheduling)
        {
            Agent.DebugMessage(msg:$"Earliest time to provide {_fArticle.Article.Name} {_fArticle.Key} at {earliestStartForForwardScheduling}");
            var msg = BasicInstruction.JobForwardEnd.Create(message: earliestStartForForwardScheduling, target: Agent.VirtualParent);
            Agent.Send(instruction: msg);
        }

        internal void ProvideRequest(FArticleProvider fArticleProvider)
        {
            // TODO: Might be problematic due to inconsistent _fArticle != Storage._fArticle
            Agent.DebugMessage(msg: $"Request for {_fArticle.Article.Name} {_fArticle.Key} provided from " + Agent.Sender);

            _fArticle = _fArticle.SetProvided.UpdateFinishedAt(f: Agent.CurrentTime);
            Agent.Send(instruction: BasicInstruction.ProvideArticle
                                                    .Create(message: fArticleProvider
                                                            ,target: Agent.VirtualParent 
                                                           ,logThis: false));
        }

        internal void WithdrawArticleFromStock()
        {
            Agent.DebugMessage(msg: $"Withdraw article {_fArticle.Article.Name} {_fArticle.Key} from Stock exchange {_fArticle.StockExchangeId}");
            Agent.Send(instruction: Storage.Instruction.WithdrawArticle
                              .Create(message: _fArticle.StockExchangeId
                                     , target: _fArticle.StorageAgent));
            Agent.TryToFinish();
        }
    }
}
