using System;
using System.Collections.Generic;
using Master40.DB.DataModel;
using Microsoft.EntityFrameworkCore.Internal;
using Zpp.DemandDomain;

namespace Zpp.MachineDomain
{
    public class MachineManager : IMachineManager
    {
        public static void JobSchedulingWithGifflerThompson(IDbTransactionData dbTransactionData,
            IDbMasterDataCache dbMasterDataCache)
        {
            /*2 Mengen:
             R: enthält die zubelegenden Maschinen (resources)
             S: einplanbare Operationen 
             */
            ISet<Machine> machines;
            ISet<ProductionOrderOperation> schedulableOperations =
                new Set<ProductionOrderOperation>();
            // must only contain unstarted operations (= schedulable),
            // which is not the case, will be sorted out in loop (performance reason)
            schedulableOperations.AddAll(dbTransactionData.ProductionOrderOperationGetAll());

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
            IGraph<INode> orderGraph = new OrderGraph(dbTransactionData);
            // build up stacks of ProductionOrderOperations
            Paths<ProductionOrderOperation> productionOrderOperationPaths =
                new Paths<ProductionOrderOperation>();
            foreach (var customerOrderPart in dbMasterDataCache.T_CustomerOrderPartGetAll().GetAll()
            )
            {
                productionOrderOperationPaths.AddAll(
                    TraverseDepthFirst((CustomerOrderPart) customerOrderPart, orderGraph));
            }

            // start algorithm
            while (schedulableOperations.Any())
            {
                machines = new Set<Machine>();
                foreach (var productionOrderOperationOfLastLevel in productionOrderOperationPaths
                    .PopLevel())
                {
                    List<Machine> machinesToAdd = productionOrderOperationOfLastLevel.GetMachines();

                    machines.AddAll(machinesToAdd);
                }

                while (machines.Any())
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
                    Machine machine = machines.PopAny();
                    // TODO prioRule
                    
                }
            }

            // Quelle: Sonnleithner_Studienarbeit_20080407 S. 8
        }

        private static Paths<ProductionOrderOperation> TraverseDepthFirst(
            CustomerOrderPart startNode, IGraph<INode> orderGraph)
        {
            var stack = new Stack<INode>();
            Paths<ProductionOrderOperation> productionOrderOperationPaths =
                new Paths<ProductionOrderOperation>();

            Dictionary<INode, bool> discovered = new Dictionary<INode, bool>();
            Stack<ProductionOrderOperation> traversedOperations =
                new Stack<ProductionOrderOperation>();

            stack.Push(startNode);
            INode parentNode;

            while (stack.Any())
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
                    if (poppedNode.GetEntity().GetType() == typeof(ProductionOrderBom))
                    {
                        ProductionOrderOperation productionOrderOperation =
                            ((ProductionOrderBom) poppedNode.GetEntity())
                            .GetProductionOrderOperation();
                        traversedOperations.Push(productionOrderOperation);
                    }

                    discovered[poppedNode] = true;
                    List<INode> childNodes = orderGraph.GetChildNodes(poppedNode);

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