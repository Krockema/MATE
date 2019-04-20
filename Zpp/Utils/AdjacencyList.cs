using System;
using System.Collections.Generic;
using System.Linq;

namespace Zpp.Utils
{
    /**
     * This is a wrapper for a generic dict representing an adjacencyList,
     * since methods with generic dict parameter are not working e.g. Dictionary<int, IEnumerable<TNode>
     */
    public class AdjacencyList<TKey, TValue>
    {
        private readonly Dictionary<TKey, List<TValue>> _adjacencyList;

        public AdjacencyList(Dictionary<TKey, TValue[]> dictionary)
        {
            _adjacencyList = new Dictionary<TKey, List<TValue>>();
            foreach (TKey key in dictionary.Keys)
            {
                _adjacencyList.Add(key, new List<TValue>(dictionary[key]));
            }
        }
        
        public AdjacencyList(Dictionary<TKey, List<TValue>> dictionary)
        {
            _adjacencyList = dictionary;
        }

        public Dictionary<TKey, List<TValue>> getAdjacencyList()
        {
            return _adjacencyList;
        }
        
        /**
         * prints the articleTree in following format (adjacencyList): parentId: child1, child2, ...
         */
        public override string ToString()
        {
            string myString = "";
            foreach (TKey rowId in _adjacencyList.Keys)
            {
                if (!_adjacencyList[rowId].Any())
                {
                    continue;
                }
                myString += rowId + ": ";
                foreach (TValue node in _adjacencyList[rowId])
                {
                    myString += node.ToString() + ", ";
                }
 
                myString = myString.Substring(0, myString.Length-2);
                myString += Environment.NewLine;
            }

            return myString;
        }
    }
}