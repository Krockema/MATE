using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Microsoft.EntityFrameworkCore.Internal;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.DbCache;
using Zpp.OrderGraph;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;

namespace Zpp.MrpRun.MachineManagement
{
    public class MachineManager : IMachineManager
    {
        public static void JobSchedulingWithGifflerThompson(IDbTransactionData dbTransactionData,
            IDbMasterDataCache dbMasterDataCache, IPriorityRule priorityRule)
        {
            /*2 Mengen:
             R: enthält die zubelegenden Maschinen (resources)
             S: einplanbare Operationen 
             */
            IStackSet<Machine> machinesSetR;
            IStackSet<ProductionOrderOperation> schedulableOperations =
                new StackSet<ProductionOrderOperation>();
            // must only contain unstarted operations (= schedulable),
            // which is not the case, will be sorted out in loop (performance reason)
            schedulableOperations.PushAll(dbTransactionData.ProductionOrderOperationGetAll());

            /* R_k hat Attribute (Maschine):
             g_j: Startzeitgrenze = _idleStartTime, ist der früheste Zeitpunkt, an dem eine Operation auf der Maschine starten kann
 
             S_k hat Attribute: (ProductionOrderOperation)
             t_i,j: Duration
             s_i,j:  Start
 
             1. Initialisierung
             g_j aller Maschinen auf 0 setzen
             s_i,j aller Operationen auf 0 setzen
             Die einplanbaren Operationen eines jeden Jobs J_i werden in die Menge S aufgenommen 
             (unterste Ebene hoch bis die erste Op kommt --> Baum von Operationen).
 
             2. Durchführung
             while S not empty:
                 alle Ops die am frühesten den Endtermin haben, füge alle benötigten Maschinen 
                 mit ihrer auslösenden Operationen in R ein.
 
                 (Die Maschinen M_j, die als nächstes belegt werden sollen, werden ausgewählt. 
                 Dazu werden zunächst die Operationen O_kr bestimmt, deren aktuell vorläufige 
                 Endzeitpunk-te am frühesten liegen. Alle Maschinen, auf denen diese ausgewählten 
                 Operationen bearbeitet werden sollen, werden in die Menge R aufgenommen. Die 
                 Operationen O_kr,die die Auswahl ausgelöst hat, wird mit der Maschine gespeichert.)
                 */

            // init
            IDirectedGraph<INode> orderDirectedGraph =
                new DemandToProviderDirectedGraph(dbTransactionData);
            // build up stacks of ProductionOrderOperations
            Paths<ProductionOrderOperation> productionOrderOperationPaths =
                new Paths<ProductionOrderOperation>();
            foreach (var customerOrderPart in dbMasterDataCache.T_CustomerOrderPartGetAll().GetAll()
            )
            {
                productionOrderOperationPaths.AddAll(TraverseDepthFirst(
                    (CustomerOrderPart) customerOrderPart, orderDirectedGraph, dbTransactionData));
            }

            // start algorithm
            while (schedulableOperations.Any())
            {
                // collect machines which have the earliest dueTime
                machinesSetR = new StackSet<Machine>();
                foreach (var productionOrderOperationOfLastLevel in productionOrderOperationPaths
                    .PopLevel())
                {
                    List<Machine> machinesToAdd =
                        productionOrderOperationOfLastLevel.GetMachines(dbTransactionData);
                    machinesSetR.PushAll(machinesToAdd);
                }

                while (machinesSetR.Any())
                {
                    /*while R not empty:
                        Entnehme R eine Maschine r (soll aus R entfernt werden)
                        Menge K: alle Ops der Maschine r
                        Wähle aus K eine Operation O_lr mittels einer Prioritätsregel aus, die folgende 
                        Eigenschaft erfüllt:
                          s_lr < (s_kr+t_kr), da sonst die Operation O_kr noch vor der Operation O_lr liegt
                        O_lr aus S entnehmen, diese gilt als geplant, der vorläufige Startzeitpunkt s_ij wird somit endgültig
                        Alle übrigen Operationen der Maschine r: addiere Laufzeit t_ij von O_lr auf s_ij der Operationen, addiere Laufzeit t_ij auf g_r von Maschine r
                    */
                    Machine machine_r = machinesSetR.PopAny();
                    // priorityRule.GetPriorityOfProductionOrderOperation(, prod, dbTransactionData);
                }
            }

            // Quelle: Sonnleithner_Studienarbeit_20080407 S. 8
        }

        public static void JobSchedulingWithGifflerThompsonAsZaepfel(
            IDbTransactionData dbTransactionData, IDbMasterDataCache dbMasterDataCache,
            IPriorityRule priorityRule)
        {
            IDirectedGraph<INode> productionOrderGraph =
                new ProductionOrderDirectedGraph(dbTransactionData, false);

            Dictionary<ProductionOrder, IDirectedGraph<INode>> productionOrderOperationGraphs =
                new Dictionary<ProductionOrder, IDirectedGraph<INode>>();
            foreach (var productionOrder in dbTransactionData.ProductionOrderGetAll())
            {
                IDirectedGraph<INode> productionOrderOperationGraph =
                    new ProductionOrderOperationDirectedGraph(dbTransactionData,
                        (ProductionOrder) productionOrder);
                productionOrderOperationGraphs.Add((ProductionOrder) productionOrder,
                    productionOrderOperationGraph);
            }

            Dictionary<Id, List<Machine>> machinesByMachineGroupId =
                new Dictionary<Id, List<Machine>>();
            foreach (var machineGroup in dbMasterDataCache.M_MachineGroupGetAll())
            {
                machinesByMachineGroupId.Add(machineGroup.GetId(),
                    dbMasterDataCache.M_MachineGetAllByMachineGroupId(machineGroup.GetId()));
            }

            /*
            S: Menge der aktuell einplanbaren Arbeitsvorgänge
            a: Menge der technologisch an erster Stelle eines Fertigungsauftrags stehenden Arbeitsvorgänge
            N(o): Menge der technologisch direkt nachfolgenden Arbeitsoperationen von Arbeitsoperation o
            M(o): Maschine auf der die Arbeitsoperation o durchgeführt wird
            K: Konfliktmenge (die auf einer bestimmten Maschine gleichzeitig einplanbaren Arbeitsvorgänge)            
            p(o): Bearbeitungszeit von Arbeitsoperation o (=Duration)
            t(o): Startzeit der Operation o (=Start)
            d(o): Fertigstellungszeitpunkt von Arbeitsoperation o (=End)
            d_min: Minimum der Fertigstellungszeitpunkte
            o_min: Operation mit minimalem Fertigstellungszeitpunkt
            o1: beliebige Operation aus K (o_dach bei Zäpfel)
            */
            IStackSet<ProductionOrderOperation> S = new StackSet<ProductionOrderOperation>();

            // Bestimme initiale Menge: S = a
            S = CreateS(productionOrderGraph, productionOrderOperationGraphs);

            // t(o) = 0 für alle o aus S
            foreach (var o in S.GetAll())
            {
                o.GetValue().Start = o.GetValue().StartBackward.GetValueOrDefault();
            }

            // while S not empty do
            while (S != null && S.Any())
            {
                int d_min = Int32.MaxValue;
                ProductionOrderOperation o_min = null;
                foreach (var o in S.GetAll())
                {
                    // Berechne d(o) = t(o) + p(o) für alle o aus S
                    o.GetValue().End = o.GetValue().Start + o.GetValue().Duration;
                    // Bestimme d_min = min{ d(o) | o aus S }
                    if (o.GetValue().End < d_min)
                    {
                        d_min = o.GetValue().End;
                        o_min = o;
                    }
                }

                // Bilde Konfliktmenge K = { o | o aus S UND M(o) == M(o_min) UND t(o) < d_min }
                IStackSet<ProductionOrderOperation> K = new StackSet<ProductionOrderOperation>();
                foreach (var o in S.GetAll())
                {
                    if (o.GetValue().MachineGroupId.Equals(o_min.GetValue().MachineGroupId) &&
                        o.GetValue().Start < d_min)
                    {
                        K.Push(o);
                    }
                }

                // while K not empty do
                if (K.Any())
                {
                    // Entnehme Operation mit höchster Prio (o1) aus K und plane auf nächster freier Machine ein

                    List<ProductionOrderOperation> allO1 = new List<ProductionOrderOperation>();

                    foreach (var machine in machinesByMachineGroupId[o_min.GetMachineGroupId()]
                        .OrderBy(x => x.GetIdleStartTime().GetValue()))
                    {
                        if (K.Any() == false)
                        {
                            break;
                        }

                        ProductionOrderOperation o1 = null;
                        o1 = priorityRule.GetHighestPriorityOperation(machine.GetIdleStartTime(),
                            K.GetAll(), dbTransactionData);
                        if (o1 == null)
                        {
                            throw new MrpRunException("This is not possible if K.Any() is true.");
                        }

                        allO1.Add(o1);

                        K.Remove(o1);

                        o1.SetMachine(machine);
                        // correct op start time if machine's idleTime is later
                        if (machine.GetIdleStartTime().GetValue() > o1.GetValue().Start)
                        {
                            o1.GetValue().Start = machine.GetIdleStartTime().GetValue();
                            o1.GetValue().End = o1.GetValue().Start + o1.GetValue().Duration;
                        }

                        machine.SetIdleStartTime(new DueTime(o1.GetValue().End));
                    }


                    // t(o) = d(o1) für alle o aus K ohne alle o1 
                    foreach (var o in K.GetAll())
                    {
                        o.GetValue().Start = allO1[0].GetValue().End;
                    }

                    /*if N(o1) not empty then
                        S = S vereinigt N(o1) ohne alle o1
                     */
                    foreach (var o1 in allO1)
                    {
                        ProductionOrder productionOrder = o1.GetProductionOrder(dbTransactionData);
                        ProductionOrderOperationDirectedGraph productionOrderOperationGraph =
                            (ProductionOrderOperationDirectedGraph) productionOrderOperationGraphs[
                                productionOrder];

                        INodes predecessorNodes =
                            productionOrderOperationGraph.GetPredecessorNodes(o1);
                        IStackSet<INode> N = null;
                        if (predecessorNodes.Any())
                        {
                            N = new StackSet<INode>(predecessorNodes);
                        }

                        // t(o) = d(o1) für alle o aus N(o1)
                        if (N != null)
                        {
                            AdaptPredecessorNodes(N, o1, productionOrderGraph,
                                productionOrderOperationGraphs);
                        }

                        // prepare for next round
                        productionOrderOperationGraph.RemoveNode(o1);
                        productionOrderOperationGraph
                            .RemoveProductionOrdersWithNoProductionOrderOperationsFromProductionOrderGraph(
                                productionOrderGraph, productionOrder);
                    }

                    S = CreateS(productionOrderGraph, productionOrderOperationGraphs);
                }
            }
        }

        /**
         * Does the following: t(o) = d(o1) für alle o aus N(o1)
         */
        private static void AdaptPredecessorNodes(IEnumerable<INode> N, ProductionOrderOperation o1,
            IDirectedGraph<INode> productionOrderGraph,
            Dictionary<ProductionOrder, IDirectedGraph<INode>> productionOrderOperationGraphs)
        {
            foreach (var node in N)
            {
                if (node.GetEntity().GetType() == typeof(ProductionOrderOperation))
                {
                    ProductionOrderOperation productionOrderOperation =
                        (ProductionOrderOperation) node.GetEntity();
                    // adapt only if o1's end is later than currentOperation else scheduled time from backwards-scheduling will be ignored
                    if (o1.GetValue().End > productionOrderOperation.GetValue().Start)
                    {
                        productionOrderOperation.GetValue().Start = o1.GetValue().End;    
                    }
                }
                else
                    // it's a Production Order --> root node
                {
                    INodes predecessorProductionOrders =
                        productionOrderGraph.GetPredecessorNodes(node);
                    if (predecessorProductionOrders == null ||
                        predecessorProductionOrders.Any() == false)
                    {
                        continue;
                    }

                    foreach (var predecessorProductionOrder in predecessorProductionOrders)
                    {
                        ProductionOrder predecessorProductionOrderTyped =
                            (ProductionOrder) predecessorProductionOrder.GetEntity();
                        IEnumerable<INode> newN =
                            productionOrderOperationGraphs[predecessorProductionOrderTyped]
                                .GetAllUniqueNode();
                        AdaptPredecessorNodes(newN, o1, productionOrderGraph,
                            productionOrderOperationGraphs);
                    }
                }
            }
        }

        /**
         * @return: all leafs of all operationGraphs
         */
        public static IStackSet<ProductionOrderOperation> CreateS(
            IDirectedGraph<INode> productionOrderGraph,
            Dictionary<ProductionOrder, IDirectedGraph<INode>> productionOrderOperationGraphs)
        {
            IStackSet<ProductionOrderOperation> S = new StackSet<ProductionOrderOperation>();
            INodes leafNodes = productionOrderGraph.GetLeafNodes(); 
            if (leafNodes == null)
            {
                return null;
            }

            INodes leafs = new Nodes();

            foreach (var productionOrder in leafNodes)
            {
                var productionOrderOperationGraph =
                    productionOrderOperationGraphs[(ProductionOrder) productionOrder.GetEntity()];
                var productionOrderOperationLeafsOfProductionOrder =
                    productionOrderOperationGraph.GetLeafNodes();

                S.PushAll(productionOrderOperationLeafsOfProductionOrder.Select(x =>
                    (ProductionOrderOperation) x.GetEntity()));
            }

            return S;
        }

        private static Paths<ProductionOrderOperation> TraverseDepthFirst(
            CustomerOrderPart startNode, IDirectedGraph<INode> orderDirectedGraph,
            IDbTransactionData dbTransactionData)
        {
            var stack = new Stack<INode>();
            Paths<ProductionOrderOperation> productionOrderOperationPaths =
                new Paths<ProductionOrderOperation>();

            Dictionary<INode, bool> discovered = new Dictionary<INode, bool>();
            Stack<ProductionOrderOperation> traversedOperations =
                new Stack<ProductionOrderOperation>();

            stack.Push(startNode);
            INode parentNode;

            while (EnumerableExtensions.Any(stack))
            {
                INode poppedNode = stack.Pop();

                // init dict if node not yet exists
                if (!discovered.ContainsKey(poppedNode))
                {
                    discovered[poppedNode] = false;
                }

                // if node is not discovered
                if (!discovered[poppedNode])
                {
                    if (poppedNode.GetType() == typeof(ProductionOrderBom))
                    {
                        ProductionOrderOperation productionOrderOperation =
                            ((ProductionOrderBom) poppedNode).GetProductionOrderOperation(
                                dbTransactionData);
                        traversedOperations.Push(productionOrderOperation);
                    }

                    discovered[poppedNode] = true;
                    INodes childNodes = orderDirectedGraph.GetSuccessorNodes(poppedNode);

                    // action
                    if (childNodes == null)
                    {
                        productionOrderOperationPaths.AddPath(traversedOperations);
                        traversedOperations = new Stack<ProductionOrderOperation>();
                    }

                    if (childNodes != null)
                    {
                        foreach (INode node in childNodes)
                        {
                            stack.Push(node);
                        }
                    }
                }
            }

            return productionOrderOperationPaths;
        }
    }
}