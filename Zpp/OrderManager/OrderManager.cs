using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.DataModel;


namespace Zpp.OrderManager
{
    public class OrderManager
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public OrderManager()
        {
            
        }

        public void Order(List<T_CustomerOrder> customerOrders)
        {
            T_CustomerOrder order = customerOrders[0];
            LOGGER.Info("Starting: with Order " + order.Id);
            
            // order.OrderParts
        }
        
        /*
            Input: A graph G and a vertex v of G

            Output: All vertices reachable from v labeled as discovered 
            
            procedure DFS-iterative(G,v):
        2      let S be a stack
        3      S.push(v)
        4      while S is not empty
        5          v = S.pop()
        6          if v is not labeled as discovered:
        7              label v as discovered
        8              for all edges from v to w in G.adjacentEdges(v) do 
        9                  S.push(w)
         */

        public void traverseDepthFirst(T_CustomerOrder order)
        {
            var stack = new Stack();
            stack.Push(order);
            
        }
        
    }
}