using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Agents.DirectoryAgent;
using Mate.Production.Core.Agents.StorageAgent;
using Mate.Production.Core.Helper;
using NLog;
using static Mate.Production.Core.Helper.Negate;
using static Mate.Production.Core.Agents.Guardian.Instruction;
using static Mate.Production.Core.Agents.SupervisorAgent.Supervisor.Instruction;
using static Mate.Production.Core.Agents.StorageAgent.Storage.Instruction.Default;
using Mate.Production.Core.Environment.Records;
using System.Collections.Immutable;

namespace Mate.Production.Core.Agents.DispoAgent.Behaviour
{
    public class Default : Types.Behaviour
    {
        internal Default(SimulationType simulationType = SimulationType.Default)
            : base(childMaker: null, simulationType: simulationType)
        {
            fArticlesToProvide = new Dictionary<IActorRef, Guid>();
        }


        internal ArticleRecord _fArticle { get; set; }

        internal Dictionary<IActorRef, Guid> fArticlesToProvide;

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
                case BasicInstruction.ProvideArticle r: ProvideRequest(ArticleProvider: r.GetObjectFromMessage); break;
                case BasicInstruction.UpdateCustomerDueTimes msg: UpdateCustomerDueTimes(msg.GetObjectFromMessage); break;
                case BasicInstruction.RemoveVirtualChild msg: RemoveVirtualChild(); break;
                case BasicInstruction.RemovedChildRef msg: TryToFinish(); break;
                default: return false;
            }
            return true;
        }
        internal void UpdateCustomerDueTimes(DateTime customerDue)
        {
            // Save Request Item.
            _fArticle = _fArticle.UpdateCustomerDue(customerDue);
            foreach (var productionRef in Agent.VirtualChildren)
            {
                Agent.Send(BasicInstruction.UpdateCustomerDueTimes.Create(customerDue, productionRef));
            }
        }

        internal void RemoveVirtualChild()
        {
            Agent.VirtualChildren.Remove(Agent.Sender);
            Agent.Send(BasicInstruction.RemovedChildRef.Create(Agent.Sender));
            TryToRemoveChildRefFromProduction();
        }
        internal void RequestArticle(ArticleRecord requestArticle)
        {
            // Save Request Item.
            _fArticle = requestArticle;
            Agent.DebugMessage($"{_fArticle.Article.Name} {_fArticle.Key} is Requested to Produce.", CustomLogger.STOCK, LogLevel.Warn);
            // get related Storage Agent
            Agent.Send(instruction: Directory.Instruction.Default.RequestAgent
                                    .Create(discriminator: requestArticle.Article.Name
                                        , target: Agent.ActorPaths.StorageDirectory.Ref));
        }

        internal void ResponseFromDirectory(AgentInformationRecord hubInfo)
        {
            Agent.DebugMessage(msg: "Acquired stock Agent: " + hubInfo.Ref.Path.Name + " from " + Agent.Sender.Path.Name, CustomLogger.INITIALIZE, LogLevel.Warn);

            _fArticle = _fArticle.UpdateStorageAgent(s: hubInfo.Ref);
            // Create Request to Storage Agent 
            Agent.Send(instruction: Storage.Instruction.Default.RequestArticle.Create(message: _fArticle, target: hubInfo.Ref));
        }

        internal void ResponseFromStock(StockReservationRecord reservation)
        {
            _fArticle = _fArticle.UpdateStockExchangeId(i: reservation.TrackingId);
           
            _quantityToProduce = _fArticle.Quantity - reservation.Quantity;

            Agent.DebugMessage(msg: reservation.Quantity + " " 
                                + _fArticle.Article.Name + " are reserved and " 
                                + _quantityToProduce + " " + _fArticle.Article.Name + " need to be produced!", CustomLogger.STOCK, LogLevel.Warn);

            if (reservation.IsInStock && !_fArticle.Article.ToBuild)
            {
                Agent.DebugMessage(msg: $"Start forward scheduling for article: {_fArticle.Article.Name} {_fArticle.Key} at: {Agent.CurrentTime}", CustomLogger.SCHEDULING, LogLevel.Warn);
                PushForwardTimeToParent(earliestStartForForwardScheduling: Agent.CurrentTime);
            }


            if (reservation.IsInStock && IsNot(_fArticle.IsHeadDemand))
            {
                ProvideRequest(new ArticleProviderRecord(ArticleKey: _fArticle.Key
                                                  , ArticleName: _fArticle.Article.Name
                                                  , StockExchangeId: reservation.TrackingId
                                                  , ArticleFinishedAt: Agent.CurrentTime
                                                  , CustomerDue: _fArticle.CustomerDue
                                                  , Provider: ImmutableHashSet.Create(new StockProviderRecord(reservation.TrackingId, "In Stock"))));
            }

            // else create Production Agents if ToBuild
            if (_fArticle.Article.ToBuild)
            {
                Agent.Send(instruction: RequestArticleBom.Create(message: _fArticle.Article.Id, target: Agent.ActorPaths.SystemAgent.Ref));

                // and request the Article from  stock at Due Time
                if (_fArticle.IsHeadDemand)
                {
                    var nextRequestAt = _fArticle.DueTime - Agent.CurrentTime;
                    Agent.DebugMessage(msg: $"Ask storage for Article {_fArticle.Key} in + {nextRequestAt}", CustomLogger.STOCK, LogLevel.Warn);

                    Agent.Send(instruction: ProvideArticleAtDue.Create(message: _fArticle.Key, target: _fArticle.StorageAgent)
                                 , waitFor: nextRequestAt.ToTimeSpan());
                }
            }
            // Not in Stock and Not ToBuild Agent has to Wait for stock to provide materials
        }

        internal void ResponseFromSystemForBom(M_Article article)
        {
            // Update
            var dueTime = _fArticle.DueTime;

            if (article.Operations != null)
                dueTime = _fArticle.DueTime - article.Operations.Sum(x => x.Duration + x.AverageTransitionDuration);
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

        private void PushForwardTimeToParent(DateTime earliestStartForForwardScheduling)
        {
            Agent.DebugMessage(msg:$"Earliest time to provide {_fArticle.Article.Name} {_fArticle.Key} at {earliestStartForForwardScheduling}", CustomLogger.SCHEDULING, LogLevel.Warn);
            var msg = BasicInstruction.JobForwardEnd.Create(message: earliestStartForForwardScheduling, target: Agent.VirtualParent);
            Agent.Send(instruction: msg);
        }

        internal void ProvideRequest(ArticleProviderRecord ArticleProvider)
        {
            // TODO: Might be problematic due to inconsistent _fArticle != Storage._fArticle
            Agent.DebugMessage(msg: $"Request for {_fArticle.Quantity} {_fArticle.Article.Name} {_fArticle.Key} provided from {Agent.Sender.Path.Name} to {Agent.VirtualParent.Path.Name}", CustomLogger.STOCK, LogLevel.Warn);

            _fArticle = _fArticle.UpdateFinishedAt(f: Agent.CurrentTime);

            var providedArticles = ArticleProvider.Provider.Select(x => x.ProvidesArticleKey);
            foreach (var articlesInProduction in fArticlesToProvide)
            {
                if (IsFalse(providedArticles.Contains(articlesInProduction.Value)) // is not original provider
                    && Agent.VirtualChildren.Contains(articlesInProduction.Key)) // and ist not already finished to produce
                {
                        Agent.Send(BasicInstruction.UpdateCustomerDueTimes
                            .Create(ArticleProvider.CustomerDue, articlesInProduction.Key));
                }
            }

            Agent.Send(instruction: BasicInstruction.ProvideArticle
                                                    .Create(message: ArticleProvider
                                                            ,target: Agent.VirtualParent 
                                                           ,logThis: false));
            
        }
        internal void WithdrawArticleFromStock()
        {
            Agent.DebugMessage(msg: $"Withdraw article {_fArticle.Article.Name} {_fArticle.Key} from Stock exchange {_fArticle.StockExchangeId}", CustomLogger.STOCK, LogLevel.Warn);
            Agent.Send(instruction: WithdrawArticle
                              .Create(message: _fArticle.StockExchangeId
                                     , target: _fArticle.StorageAgent));
            _fArticle = _fArticle.SetProvided();
            TryToRemoveChildRefFromProduction();
        }

        private void TryToRemoveChildRefFromProduction()
        {
            if (_fArticle.IsHeadDemand)
            {
                TryToFinish();
                return;
            }

            if (Agent.VirtualChildren.Count == 0 && _fArticle.IsProvided)
            {
                Agent.Send(BasicInstruction.RemoveVirtualChild.Create(Agent.VirtualParent));
            }
        }

        internal void TryToFinish()
        {
            if (Agent.VirtualChildren.Count == 0 && _fArticle.IsProvided)
            {
                Agent.DebugMessage(
                    msg: $"Shutdown for {_fArticle.Article.Name} " +
                         $"(Key: {_fArticle.Key}, OrderId: {_fArticle.CustomerOrderId})"
                    , CustomLogger.DISPOPRODRELATION, LogLevel.Debug);
                Agent.TryToFinish();
                return;
            }

            Agent.DebugMessage(
                msg: $"Could not run shutdown for {_fArticle.Article.Name} " +
                     $"(Key: {_fArticle.Key}, OrderId: {_fArticle.CustomerOrderId}) cause article is provided {_fArticle.IsProvided } and has childs left  {Agent.VirtualChildren.Count} "
                , CustomLogger.DISPOPRODRELATION, LogLevel.Debug);
        }
        public override void OnChildAdd(IActorRef childRef)
        {
            var articleKey = _fArticle.Keys.ToArray()[fArticlesToProvide.Count];
            var baseArticle = _fArticle;
            fArticlesToProvide.Add(Agent.Context.Sender, articleKey);
            Agent.Send(instruction: ProductionAgent.Production.Instruction.StartProduction.Create(message: baseArticle, target: Agent.Context.Sender));
            Agent.DebugMessage(msg: $"Dispo<{baseArticle.Article.Name } (OrderId: { baseArticle.CustomerOrderId }) > ProductionStart has been sent for { baseArticle.Key }.");
        }
    }
}
