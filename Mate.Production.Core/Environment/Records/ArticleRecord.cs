using System;
using System.Collections.Immutable;
using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment.Records.Interfaces;

namespace Mate.Production.Core.Environment.Records
{
    public record ArticleRecord(Guid Key,
                            ImmutableHashSet<Guid> Keys,
                            DateTime CreationTime,
                            M_Article Article,
                            Guid StockExchangeId,
                            IActorRef StorageAgent,
                            int Quantity,
                            DateTime DueTime,
                            DateTime CustomerDue,
                            TimeSpan RemainingDuration,
                            IActorRef OriginRequester,
                            IActorRef DispoRequester,
                            IImmutableSet<StockProviderRecord> ProviderList,
                            int CustomerOrderId,
                            bool IsProvided,
                            DateTime ProvidedAt,
                            bool IsHeadDemand,
                            DateTime FinishedAt) : IKey
    {
        public ArticleRecord UpdateFinishedAt(DateTime f) =>
            this with { FinishedAt = f };

        public ArticleRecord UpdateProvidedAt(DateTime t) =>
            this with { ProvidedAt = t };

        public ArticleRecord UpdateOriginRequester(IActorRef r) =>
            this with { OriginRequester = r };

        public ArticleRecord UpdateDispoRequester(IActorRef r) =>
            this with { DispoRequester = r };

        public ArticleRecord UpdateCustomerOrderAndDue(int id, DateTime due, IActorRef storage) =>
            this with { CustomerOrderId = id, DueTime = due, StorageAgent = storage };

        public ArticleRecord UpdateCustomerDue(DateTime due) =>
            this with { CustomerDue = due };

        public ArticleRecord UpdateArticle(M_Article article) =>
            this with { Article = article };

        public ArticleRecord UpdateStorageAgent(IActorRef s) =>
            this with { StorageAgent = s };

        public ArticleRecord UpdateStockExchangeId(Guid i) =>
            this with { StockExchangeId = i };

        public ArticleRecord UpdateProviderList(ImmutableHashSet<StockProviderRecord> p) =>
            this with { ProviderList = p };

        public ArticleRecord SetProvided() =>
            this with { IsProvided = true };
    }
}