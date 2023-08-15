using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Agents.ProductionAgent.Types;
using Mate.Test.Online.Preparations;
using Xunit;

namespace Mate.Test.Online.Agents.Types
{
    public class OperationManagers
    {
        public OperationManagers()
        {

        }

        [Fact]
        public void AddAndRetrieveOperationByKey()
        {
            var operationManager = new OperationManager();
            var job = TypeFactory.CreateDummyOperationRecord(jobName: "Sample Operation 1", jobDuration: TimeSpan.FromMinutes(10)
                                                            , averageTransitionDuration: TimeSpan.Zero
                                                            , dueTime: Time.ZERO.Value
                                                            , customerDue: Time.ZERO.Value
                                                            , currentTime: Time.ZERO.Value);
            operationManager.AddOperation(job);
            var returnedJob = operationManager.GetByOperationKey(job.Key);

            Assert.True(returnedJob != null);
        }

        [Fact]

        public void GetOperationsByCapability()
        {
            var operationManager = new OperationManager();
            var job = TypeFactory.CreateDummyOperationRecord(jobName: "Sample Operation 1", jobDuration: TimeSpan.FromMinutes(10)
                                                            , averageTransitionDuration: TimeSpan.Zero
                                                            , dueTime: Time.ZERO.Value
                                                            , customerDue: Time.ZERO.Value
                                                            , currentTime: Time.ZERO.Value);
            var returnedJob = operationManager.GetOperationByCapability("Sewing");
            Assert.True(returnedJob != null);
        }

        [Fact]
        public void SetArticleProvided()
        {
            var operationManager = new OperationManager();
            var bom = new M_ArticleBom() { ArticleChild = new M_Article() {Name = "Wood"}, Quantity = 1};
            var job = TypeFactory.CreateDummyOperationRecord(jobName: "Sample Operation 1"
                                                            , jobDuration: TimeSpan.FromMinutes(10)
                                                            , bom: bom, averageTransitionDuration: TimeSpan.Zero
                                                            , dueTime: Time.ZERO.Value
                                                            , customerDue: Time.ZERO.Value
                                                            , currentTime: Time.ZERO.Value);
            var article = new M_Article()
            {
                Name = "Bear"
                , Operations = new List<M_Operation>() { job.Operation }
                , ArticleBoms = new List<M_ArticleBom>() { bom }
            };
            var fArticle = TypeFactory.CreateDummyArticle(Time.ZERO.Value + TimeSpan.FromMinutes(59), Time.ZERO.Value + TimeSpan.FromMinutes(59), Time.ZERO.Value, article, 1);
            operationManager.AddOperation(job);
            var numberOfRequiredDispos = operationManager.CreateRequiredArticles(fArticle, ActorRefs.Nobody, Time.ZERO.Value);
            Assert.True(numberOfRequiredDispos == 1);
        }

        [Fact]
        public void UpdateOperations() { }
        [Fact]
        public void ProvideArticle()
        {
            var operationManager = new OperationManager();
            var job = TypeFactory.CreateDummyOperationRecord(jobName: "Sample Operation 1"
                                                            , jobDuration: TimeSpan.FromMinutes(10)
                                                            , averageTransitionDuration: TimeSpan.Zero
                                                            , Time.ZERO.Value, Time.ZERO.Value, Time.ZERO.Value);
            operationManager.AddOperation(job);
            var returnedJob = operationManager.GetByOperationKey(job.Key);

            Assert.True(returnedJob != null);
        }
    }
}