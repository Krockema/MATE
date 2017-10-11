using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Master40.Agents.Agents.Internal;
using Master40.Agents.Agents.Model;
using Master40.DB.Models;
using Remotion.Linq.Utilities;

namespace Master40.Agents.Agents
{
    public class ProductionAgent : Agent
    {
        private RequestItem RequestItem { get; } // the for this Work
        private List<RequestItem> RequestMaterials { get; set; }
        private List<ComunicationAgent> ComunicationAgents;
        private List<WorkItem> WorkItems { get; set; }
        public ProductionAgent(Agent creator, string name, bool debug, RequestItem requestItem) 
            : base(creator, name, debug)
        {
            RequestItem = requestItem;
            RequestMaterials = new List<RequestItem>();
            ComunicationAgents = new List<ComunicationAgent>();
            DebugMessage("Woke up. My dueTime is :" + requestItem.DueTime);
            StartProductionAgent();
        }


        public enum InstuctionsMethods
        {
            Finished,
            ProductionStarted
        }

        private void StartProductionAgent()
        {
            var firstToEnqueue = false;
            // check for Children
            if (!RequestItem.Article.ArticleBoms.Any())
            {
                DebugMessage("Last leave in Bom");
                firstToEnqueue = true;
            }
            
            // if item hase Workschedules Request ComClient for them
            if (RequestItem.Article.WorkSchedules != null) { 
                // Ask the Directory Agent for Service
                RequestComunicationAgentFor(workSchedules: RequestItem.Article.WorkSchedules);
                // And Create workItems
                CreateWorkItemsFromRequestItem(firstItemToBuild: firstToEnqueue);
            }

            // Create Dispo Agents for Childs.
            foreach (var articleBom in RequestItem.Article.ArticleBoms)
            {
                var item = new RequestItem
                {
                    Article = articleBom.ArticleChild,
                    Quantity = Convert.ToInt32(articleBom.Quantity),
                    DueTime = RequestItem.DueTime,
                    OrderId = RequestItem.OrderId
                };

                // create Dispo Agents for to Provide Required Articles
                var dispoAgent = new DispoAgent(creator: this,
                                                system: ((StorageAgent)Creator).Creator,
                                                name: RequestItem.Article.Name + " Child of(" + this.Name + ")",
                                                debug: DebugThis,
                                                requestItem: item);
                // add to childs
                ChildAgents.Add(dispoAgent);
                //RequestMaterials.Add(item);
                // add TO Context to process them this time Period.
                Context.ProcessesRemainingThisTimePeriod.Enqueue(dispoAgent);
            }
        }

        internal new void Finished(InstructionSet instructionSet)
        {
            // any Not Finished do noting
            if (ChildAgents.Any(x => x.Status != Status.Finished))
                return;

            // Return from Production as WorkItemStatus
            var status = instructionSet.ObjectToProcess as WorkItemStatus;
            if (status != null)
            {
                var workItem = WorkItems.First(x => x.Id == status.WorkItemId);
                workItem.Status = status.Status;
            }

            // TODO Anything ?
            if (RequestItem.Article.WorkSchedules != null && WorkItems.All(x => x.Status == Status.Finished)) {
                this.Status = Status.Finished;
                CreateAndEnqueueInstuction(methodName: StorageAgent.InstuctionsMethods.ResponseFromProduction.ToString(),
                                      objectToProcess: this,
                                          targetAgent: this.Creator); // Has been injected by Dispo and is Storage
                

                DebugMessage("All Workschedules have been Finished");
                return;
            }
            // else
            DebugMessage("Im Ready To get Enqued");
            Status = (Status== Status.Processed)? Status.Processed : Status.Ready;
            WorkItems.ForEach(item => item.MaterialsProvided = true);
            SetWorkItemReady();
        }

        private void RequestComunicationAgentFor(IEnumerable<WorkSchedule> workSchedules)
        {
            var directoryAgent = ((StorageAgent)Creator).Creator.ChildAgents.OfType<DirectoryAgent>().FirstOrDefault();
            if (directoryAgent == null)
            {
                throw new DirectoryNotFoundException("Could not find Directory Agent.");
            }

            // Request Comunication Agent for my Workschedules
            foreach (var machineGroupName in workSchedules.Select(x => x.MachineGroup.Name).Distinct())
            {
                CreateAndEnqueueInstuction(methodName: DirectoryAgent.InstuctionsMethods.GetOrCreateComunicationAgentForType.ToString(),
                                      objectToProcess: machineGroupName,
                                          targetAgent: directoryAgent);
            }
        }

        private void SetComunicationAgent(InstructionSet instructionSet)
        {
            // Enque my Element at Comunication Agent
            var comunicationAgent = instructionSet.ObjectToProcess as ComunicationAgent;
            if (comunicationAgent == null)
            {
                throw new InvalidCastException(" Could not Cast Comunication agent on InstructionSet.");
            }
            DebugMessage("Recived Agent from Directory: " + comunicationAgent.Name );
            
            // add agent to current Scope.
            ComunicationAgents.Add(comunicationAgent);
            // foreach fitting WorkSchedule
            foreach (var workItem in WorkItems.Where(x => x.WorkSchedule.MachineGroup.Name == comunicationAgent.ContractType))
            {
                CreateAndEnqueueInstuction(methodName: ComunicationAgent.InstuctionsMethods.EnqueueWorkItem.ToString(),
                                      objectToProcess: workItem,
                                          targetAgent: comunicationAgent);
            }
        }

        private void CreateWorkItemsFromRequestItem(bool firstItemToBuild)
        {
            WorkItems = new List<WorkItem>();
            int lastdue = RequestItem.DueTime;
            foreach (var workSchedule in RequestItem.Article.WorkSchedules.OrderBy(x => x.HierarchyNumber))
            {
                var n = new WorkItem
                {
                    Status = firstItemToBuild ? Status.Ready:Status.Created,
                    DueTime = lastdue,
                    WorkSchedule = workSchedule,
                    ProductionAgent = this,
                    Priority = PriorityRules.ActivitySlack(currentTime: (int)Context.TimePeriod, 
                                                      processDuration: workSchedule.Duration, 
                                                           processDue: lastdue)
                };
               

                DebugMessage("Created WorkItem: " + workSchedule.Name + " | Due:" + lastdue + " | Status: " + n.Status);
                lastdue = lastdue - workSchedule.Duration;
                firstItemToBuild = false;
                WorkItems.Add(n);
                Statistics.CreateSimulationWorkSchedule(n, RequestItem.OrderId.ToString(), RequestItem.IsHeadDemand);
            }
        }

        private void  SetWorkItemReady()
        {
            // check if there is something Ready or in Process Then just wait for their Ready Call
            if (WorkItems.Any(x => x.Status == Status.Ready || x.Status == Status.Processed))
                return;

            // get next ready WorkItem
            // TODO Return Queing Status ? or Move method to Machine
            var nextItem = WorkItems.Where(x => x.Status == Status.InQueue || x.Status == Status.Created)
                                    .OrderBy(x => x.WorkSchedule.HierarchyNumber)
                                    .FirstOrDefault();
            if (nextItem == null)
            {
                DebugMessage("Cannot start next.");
                return;
            }

            var comunicationAgent = ComunicationAgents.FirstOrDefault(x => x.ContractType == nextItem.WorkSchedule.MachineGroup.Name);

            DebugMessage("SetFirstWorkItemReady From Status " + nextItem.Status + " Time " + Context.TimePeriod);

            nextItem.Status = Status.Ready;
            nextItem.WasSetReady = true;
            // create StatusMsg
            var message = new WorkItemStatus
            {
                WorkItemId = nextItem.Id,
                CurrentPriority = nextItem.Priority,  // TODO MAY NEED TO RECALCULATE IN FUTURE
                Status = Status.Ready,                // TODO: MAybe need to change to an Extra Field -> bool IsReady 
            };

            // tell Item in Queue to set it ready.
            CreateAndEnqueueInstuction(methodName: ComunicationAgent.InstuctionsMethods.SetWorkItemStatus.ToString(),
                                  objectToProcess: message,
                                      targetAgent: comunicationAgent);
            // ,waitFor: 1); // Start Production during the next time period
        }


        private void FinishWorkItem(InstructionSet instructionSet)
        {
            var workItem = instructionSet.ObjectToProcess as WorkItem;
            if (workItem == null)
            {
                throw new InvalidCastException("Could not Cast >WorkItemStatus< on InstructionSet.ObjectToProcess");
            }

            DebugMessage("Machine called finished with: " + workItem.WorkSchedule.Name + " !");
            CreateAndEnqueueInstuction(methodName: ProductionAgent.InstuctionsMethods.Finished.ToString(),
                                  objectToProcess: workItem,
                                      targetAgent: workItem.ProductionAgent);
        }

        private void ProductionStarted(InstructionSet instructionSet)
        {
            var workItem = instructionSet.ObjectToProcess as WorkItem;
            if (this.Status != Status.Processed)
            {
                this.Status = Status.Processed;
                foreach (var agent in ChildAgents)
                {
                    CreateAndEnqueueInstuction(methodName: DispoAgent.InstuctionsMethods.WithdrawMaterialsFromStock.ToString(),
                                          objectToProcess: "Production Start",
                                              targetAgent: agent);
                }
            }
            
        }

    }
}
