using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;
using static FArticles;

namespace Master40.SimulationCore.Agents.ProductionAgent.Types
{
    public class DispoArticleDictionary
    {
        private Dictionary<IActorRef, FArticle> _dispoArticleDictionary = new Dictionary<IActorRef, FArticle>();

        public void Add(IActorRef dispoRef, FArticle fArticle)
        {
            _dispoArticleDictionary.Add(dispoRef, fArticle);
        }

        internal void Update(IActorRef dispoRef, FArticle fArticle)
        {
            if (!_dispoArticleDictionary.Remove(key: dispoRef))
                throw new Exception(message: "Could not find any Item to remove from DispoArticleDictionary!");
         
            if (fArticle != null) _dispoArticleDictionary.Add(key: dispoRef, value: fArticle);
        }

        internal bool AllProvided()
        {
            return _dispoArticleDictionary.All(x => x.Value.IsProvided);
        }
    }
}
