using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;

namespace Zpp.Utils
{
    public class TreeTools<Node>
    {
        /*
                Input: A graph G and a vertex v of G
            
                Output: All vertices reachable from v labeled as discovered 
                
            1    procedure DFS-iterative(G,v):
            2      let S be a stack
            3      S.push(v)
            4      while S is not empty
            5          v = S.pop()
            6          if v is not labeled as discovered:
            7              label v as discovered
            8              for all edges from v to w in G.adjacentEdges(v) do 
            9                  S.push(w)
         */
        
        // TODO: This must be revisited under following aspect: a article node can be existing multiple times in tree,
        // it must be ensured, that every multiple object have its own instance
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <returns>
        ///    The List of the traversed nodes in exact order
        /// </returns>
        public static List<Node> traverseDepthFirst(ITree<Node> tree)
        {
            var stack = new Stack<Node>();
            
            Dictionary<Node, bool> discovered = new Dictionary<Node, bool>();
            List<Node> traversed = new List<Node>();
            
            stack.Push(tree.getRootNode());
            while (stack.Any())
            {
                Node poppedNode = stack.Pop();
                traversed.Add(poppedNode);
                // if node is not discovered
                if (! discovered.ContainsKey(poppedNode) || ! discovered[poppedNode] )
                {
                    discovered[poppedNode] = true;
                    foreach (Node node in tree.getChildNodes(poppedNode))
                    {
                        stack.Push(node);
                    }
                }
            }
            return traversed;
        }
    }
}