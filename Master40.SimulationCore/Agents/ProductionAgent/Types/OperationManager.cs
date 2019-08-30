using Akka.Actor;
using Master40.SimulationCore.Agents.ProductionAgent.Types;
using Master40.SimulationCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.SimulationCore.Types;
using Microsoft.FSharp.Core;
using Remotion.Linq.Parsing;
using static FArticleProviders;
using static FArticles;
using static FOperations;
using static IJobs;

namespace Master40.SimulationCore.Agents.ProductionAgent.Types
{
    public class OperationManager
    {
        private List<DispoArticleDictionary> _articleProvider = new List<DispoArticleDictionary>();


        internal List<FOperation> GetOperations => _articleProvider.Select(x => x.Operation).ToList();
        internal List<IActorRef> GetProviderForOperation(Guid operationKey)
        {
            var articleTuple = _articleProvider.Single(x => x.Operation.Key == operationKey);
            var provider = articleTuple.DispoToArticleRelation.Select(x => x.Provider).ToList();
            return provider;
        }

        public List<FOperation> GetOperationBySkill(string skillName)
        {
            var operations = _articleProvider.Where(x => x.Operation.Operation.ResourceSkill.Name == skillName)
                                             .Select(x => x.Operation)
                                             .ToList();
            return operations;
        }

        internal FArticle Set(IActorRef provider)
        {
            // return first FArticle that has no provider
            var emptyProviders = GetAllRequiredArticlesWithoutProvider();
            var first = emptyProviders.First();
            first.Provider = provider;
            return first.Article;
        }

        private List<ArticleProvider> GetAllRequiredArticlesWithoutProvider()
        {
            List<ArticleProvider> emptyProviders = new List<ArticleProvider>();
            foreach (var provider in _articleProvider)
            {
                emptyProviders.AddRange(provider.GetWithoutProvider());
            }

            return emptyProviders;
        }

        public FOperation GetOperationByKey(Guid operationKey)
        {
            var operation = _articleProvider.Single(x => x.Operation.Key == operationKey).Operation;
            return operation;
        }

        public void UpdateOperations(List<FOperation> operations)
        {
            foreach (var operation in operations)
            { 
                var toUpdate = GetByOperationKey(operation.Key);
                toUpdate.Operation = operation;
            }
        }

        public DispoArticleDictionary GetByOperationKey(Guid operationKey)
        {
            return _articleProvider.Single(x => x.Operation.Key == operationKey);
        }

        public void AddOperation(FOperation fOperation)
        {
            _articleProvider.Add(new DispoArticleDictionary(operation: fOperation));
        }

        public int CreateRequiredArticles(FArticle articleToProduce, IActorRef requestingAgent, long currentTime)
        {
            List<ArticleProvider> listAP = new List<ArticleProvider>();
            var counter = articleToProduce.Article.ArticleBoms.Count;
            foreach (var fOperation in GetOperations) { 
                var provider = GetArticleDispoProvider(operationKey: fOperation.Key);
                if (fOperation.Operation.ArticleBoms.Count == 0)
                {
                    //TODO Log this somewhere - not in System Debug
                    //System.Diagnostics.Debug.WriteLine($"Operation {fOperation.Operation.Name} of Article {articleToProduce.Article.Name} has no RequiredArticles!");
                    fOperation.StartConditions.ArticlesProvided = true;
                }

                foreach (var bom in fOperation.Operation.ArticleBoms)
                {
                    var createdArticleProvider = new ArticleProvider(provider: ActorRefs.Nobody,
                        article: bom.ToRequestItem(requestItem: articleToProduce
                            , requester: requestingAgent
                            , currentTime: currentTime));
                    listAP.Add(createdArticleProvider);
                    provider.DispoToArticleRelation.Add(createdArticleProvider);

                }
            }
            //TODO Log this somewhere
            //System.Diagnostics.Debug.WriteLine($"ProductionAgent {articleToProduce.Article.Name} require {counter} Bom elements and therefore created {listAP.Count} Dispos.");

            

            return listAP.Count;
        }

        internal FOperation GetNextOperation(Guid key)
        {
            var currentOperation = GetOperationByKey(key);

            var nextOperation = GetOperations.Where(x => x.Operation.HierarchyNumber > currentOperation.Operation.HierarchyNumber)
                                            .OrderBy(x => x.Operation.HierarchyNumber).FirstOrDefault();

            return nextOperation;

        }

        public DispoArticleDictionary GetArticleDispoProvider(Guid operationKey)
        {
            return _articleProvider.Single(x => x.Operation.Key == operationKey);
        }
        
        public DispoArticleDictionary SetArticleProvided(FArticleProvider fArticleProvider,IActorRef providedBy, long currentTime)
        {
            foreach (var provider in _articleProvider)
            {
                var providedArticle = provider.GetByKey(fArticleProvider.ArticleKey);
                if (providedArticle == null) continue;

                providedArticle.Article = providedArticle.Article.SetProvided.UpdateFinishedAt(currentTime);
                providedArticle.Provider = providedBy;

                return provider;
            }

            throw new Exception("Article was not found from Operation Manager. We are sorry for any Inconvenience!");
        }
    }
}
