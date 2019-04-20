using System;
using System.Collections.Generic;
using System.Linq;

namespace Zpp.Utils
{
    /**
     * This is a wrapper for a generic dict representing an adjacencyList,
     * since methods with generic dict parameter are not working e.g. Dictionary<int, IEnumerable<TNode>
     */
    public class AdjacencyList<TNode>
    {
        private readonly Dictionary<TNode, List<TNode>> _adjacencyList;

        public AdjacencyList(Dictionary<TNode, TNode[]> dictionary)
        {
            _adjacencyList = new Dictionary<TNode, List<TNode>>();
            foreach (TNode key in dictionary.Keys)
            {
                _adjacencyList.Add(key, new List<TNode>(dictionary[key]));
            }
        }
        
        public AdjacencyList(Dictionary<TNode, List<TNode>> dictionary)
        {
            _adjacencyList = new Dictionary<TNode, List<TNode>>(dictionary);
        }

        public Dictionary<TNode, List<TNode>> getAsDictionary()
        {
            return _adjacencyList;
        }
        
        /**
         * prints the articleTree in following format (adjacencyList): parentId: child1, child2, ...
         */
        public override string ToString()
        {
            string myString = "";
            foreach (TNode rowId in _adjacencyList.Keys)
            {
                if (!_adjacencyList[rowId].Any())
                {
                    continue;
                }
                myString += rowId + ": ";
                foreach (TNode node in _adjacencyList[rowId])
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