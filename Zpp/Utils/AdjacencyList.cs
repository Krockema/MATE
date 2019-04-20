using System;
using System.Collections.Generic;
using System.Linq;

namespace Zpp.Utils
{
    /**
     * This is a wrapper for a generic dict representing an adjacencyList,
     * since methods with generic dict parameter are not working e.g. Dictionary<int, IEnumerable<TNode>
     */
    public class AdjacencyList<TEntity>
    {
        private readonly Dictionary<Node<TEntity>, List<Node<TEntity>>> _adjacencyList;

        public AdjacencyList(Dictionary<Node<TEntity>, Node<TEntity>[]> dictionary)
        {
            _adjacencyList = new Dictionary<Node<TEntity>, List<Node<TEntity>>>();
            foreach (Node<TEntity> key in dictionary.Keys)
            {
                _adjacencyList.Add(key, new List<Node<TEntity>>(dictionary[key]));
            }
        }
        
        public AdjacencyList(Dictionary<Node<TEntity>, List<Node<TEntity>>> dictionary)
        {
            _adjacencyList = new Dictionary<Node<TEntity>, List<Node<TEntity>>>(dictionary);
        }

        public Dictionary<Node<TEntity>, List<Node<TEntity>>> getAsDictionary()
        {
            return _adjacencyList;
        }
        
        /**
         * prints the articleTree in following format (adjacencyList): parentId: child1, child2, ...
         */
        public override string ToString()
        {
            string myString = "";
            foreach (Node<TEntity> rowId in _adjacencyList.Keys)
            {
                if (!_adjacencyList[rowId].Any())
                {
                    continue;
                }
                myString += rowId + ": ";
                foreach (Node<TEntity> node in _adjacencyList[rowId])
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