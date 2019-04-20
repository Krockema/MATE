using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;

namespace Zpp.Utils
{
    public static class TreeTools<TEntity>
    {
        // 
        // TODO: Switch this to iterative depth search (with dfs limit default set to max depth of given truck examples)
        ///
        /// <summary>
        ///     A depth-first-search (DFS) traversal of given tree
        /// </summary>
        /// <param name="tree">to traverse</param>
        /// <returns>
        ///    The List of the traversed nodes in exact order
        /// </returns>
        public static List<Node<TEntity>> traverseDepthFirst(ITree<TEntity> tree, Action<Node<TEntity>> action)
        {
            var stack = new Stack<Node<TEntity>>();
            
            Dictionary<Node<TEntity>, bool> discovered = new Dictionary<Node<TEntity>, bool>();
            List<Node<TEntity>> traversed = new List<Node<TEntity>>();
            
            stack.Push(tree.GetRootNode());
            while (stack.Any())
            {
                Node<TEntity> poppedNode = stack.Pop();
                traversed.Add(poppedNode);
                
                // init dict if node not yet exists
                if (! discovered.ContainsKey(poppedNode) )
                {
                    discovered[poppedNode] = false;
                } 
                
                // if node is not discovered
                if (! discovered[poppedNode] )
                {
                    discovered[poppedNode] = true;
                    action(poppedNode);
                    
                    foreach (Node<TEntity> node in tree.GetChildNodes(poppedNode))
                    {
                        stack.Push(node);
                    }
                }
            }
            return traversed;
        }


    }
}