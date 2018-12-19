using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Master40.Agents.Agents.Model;
using NSimulate.Instruction;
using Process = NSimulate.Process;

namespace Master40.Agents.Agents.Internal
{
    public abstract class Agent : Process
    {
        // Agent Statistics
        public static List<String> AgentCounter = new List<string>();
        public static int InstructionCounter = 0;
        public static int ItemsProduced = 0; 
        public static List<AgentStatistic> AgentStatistics = new List<AgentStatistic>();
        
        private Stopwatch _stopwatch = new Stopwatch();

        // process State
        private bool Waiting = false;

        // Agent Properties.
        public Guid AgentId { get; }
        internal Agent Creator { get; set; }
        internal List<Agent> ChildAgents { get; set; }
        public string Name { get; set; }
        public bool DebugThis { get; set; }
        internal Status Status { get; set; }
        public Queue<InstructionSet> InstructionQueue { get; set; }
        protected List<Dictionary<string, object>> allChildData = new List<Dictionary<string, object>>();

        protected Agent(Agent creator, string name, bool debug)
        {
            AgentId = Guid.NewGuid();
            AgentCounter.Add(this.GetType().Name);
            this.Name = name;
            this.DebugThis = debug;
            this.InstructionQueue = new Queue<InstructionSet>();
            this.ChildAgents = new List<Agent>();
            this.Status = Status.Created;
            
            // Cheack for Creator Agent
            var creatorsName = "Simulation Context";
            if (creator == null) { Creator = this; }
            else {  this.Creator = creator; creatorsName = Creator.Name; }

            DebugMessage(" created by " + creatorsName + ", GUID: " + AgentId);
        }

        public enum BaseInstuctionsMethods
        {
            ReturnData,
            ReceiveData
        }

        public override IEnumerator<InstructionBase> Simulate()
        {
            // while Simulation is Running
            while (true)
            {
                TimerStop();
                // Wait for Instructions
                if (InstructionQueue.Count == 0)
                {
                    yield return new WaitConditionInstruction(() => InstructionQueue.Count > 0);
                }
                // If there are Instructions
                var doTask = InstructionQueue.Dequeue();
                if (doTask.MethodName == "Wait")
                {
                    // Creates an Agent Activity that reactivates the object at the Required time.
                    var activity = new AgentActivity(instructionSet: doTask.ObjectToProcess as InstructionSet,
                                                     targetAgent: doTask.SourceAgent, 
                                                     waitFor: doTask.WaitFor);
                    yield return new ActivateInstruction(activity);
                    continue;
                }

                // Statistic
                InstructionCounter++;
                TimerStart();
                // Proceed with each one by one - Methods to call MUST be implemented by the Derived Agent itself
                var method = this.GetType().GetMethod(doTask.MethodName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (method == null)
                    throw new NotImplementedException(Name  + " | Source: " + doTask.SourceAgent + " | Method Name: " + doTask.MethodName);
                // call Method.
                var invokeReturn = method.Invoke(this, new object[] { doTask }) ;
            }
        }

        private void TimerStop()
        {
            if (_stopwatch.IsRunning)
            {
                _stopwatch.Stop();
                AgentStatistics.Add(new AgentStatistic { AgentName = this.Name,
                                                    ProcessingTime = _stopwatch.ElapsedMilliseconds,
                                                           AgentId = this.AgentId.ToString(),
                                                         AgentType = this.GetType().Name,
                                                              Time = Context.TimePeriod 
                });
            }   
        }

        private void TimerStart()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Finalize the current Agent and Call the Parrent.
        /// </summary>
        internal void Finish()
        {
            if (DebugThis)
            {
                DebugMessage(" Finished Work.");
            }
            // Set State Finish
            this.Status = Status.Finished;
            // Tell Parent
            CreateAndEnqueueInstuction(
                     methodName: "Finished",
                objectToProcess: this,
                    targetAgent: this.Creator
            );
        }


        /// <summary>
        /// check Childs and Call Finish if all in State Finished.
        /// </summary>
        internal void Finished(InstructionSet objects)
        {
            if (ChildAgents.All(x => x.Status == Status.Finished) && this.Status == Status.Finished)
            {
                ChildAgents.Clear();
                Finish();
            }
        }

        /// <summary>
        /// Creates a Instuction Set and Enqueue it to the TargetAgent,
        /// It pushes the Agent to Context.Queue
        /// ATTENTION !! BE CAERFULL WITH WAITFOR !!
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="objectToProcess"></param>
        /// <param name="targetAgent"></param>
        /// <param name="waitFor"> Creates a Agent activity which will reaactivaded the agent after the specified time Period!</param>
        /// <param name="sourceAgent"></param>
        public void CreateAndEnqueueInstuction(string methodName, object objectToProcess, Agent targetAgent, long waitFor = 0)
        {
            var instruction = new InstructionSet {  MethodName = methodName,
                                                    ObjectToProcess = objectToProcess,
                                                    ObjectType = objectToProcess.GetType(),
                                                    SourceAgent = this };

            if (waitFor == 0)
            {
                // Create And Enqueue
                targetAgent.InstructionQueue.Enqueue(instruction);
                // Re-Activate Process in Context Queue if nesessary
                if (!Context.ProcessesRemainingThisTimePeriod.Contains(targetAgent))
                    Context.ProcessesRemainingThisTimePeriod.Enqueue(targetAgent);
            }
            else
            {
                // Wrap Instruction with waiter
                WaitFor(instruction,targetAgent, waitFor);
            }

        }

        /// <summary>
        /// Impementation of debug msg broker.
        /// </summary>
        /// <param name="msg"></param>
        internal void DebugMessage(string msg)
        {
            if (DebugThis)
            {
                var logItem = "Time(" + Context.TimePeriod + ").Agent(" + Name + ") : " + msg;
                Debug.WriteLine(logItem);
                AgentStatistic.Log.Add(logItem);
            }
        }

        private void WaitFor(InstructionSet instruction , Agent targetAgent, long waitFor)
        {
            // Create And Enqueue
            this.InstructionQueue.Enqueue(new InstructionSet
            {
                MethodName = "Wait",
                ObjectToProcess = instruction,
                ObjectType = instruction.GetType(),
                SourceAgent = targetAgent,
                WaitFor = waitFor
            });
        }

        private void CollectProps(object obj, ref Dictionary<string, object> props, String prefix = "")
        {
            Type type = obj.GetType();
            foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (new[] { typeof(RequestItem), typeof(DB.Models.Machine), typeof(DB.Models.Article) }.Contains(prop.PropertyType))
                {
                    CollectProps(prop.GetValue(obj), ref props, prefix + prop.Name + "-");
                }
                //else if (prop.PropertyType == typeof(List<WorkItem>))
                //{
                //    // TODO
                //}
                else
                {
                    props.Add(prefix + prop.Name, prop.GetValue(obj));
                }
            }
        }

        private List<Dictionary<string, object>> GetData()
        {
            List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
            Dictionary<string, object> data = new Dictionary<string, object>();
            //data.Add("AgentId", this.AgentId);
            //data.Add("AgentName", this.Name);
            data.Add("AgentType", this.GetType());
            //data.Add("AgentStatus", this.Status);
            
            Type agentType = this.GetType();
            //foreach (PropertyInfo prop in agentType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            //{
            //    if (new[] { typeof(RequestItem), typeof(DB.Models.Machine) }.Contains(prop.PropertyType))
            //    {
            //        foreach (PropertyInfo subProp in prop.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            //        {
            //            data.Add(prop.Name + subProp.Name, subProp.GetValue(prop.GetValue(this)));
            //        }
            //    }
            //    else if (prop.PropertyType == typeof(List<WorkItem>))
            //    {
            //        // TODO
            //    }
            //    else
            //    {
            //        data.Add(prop.Name, prop.GetValue(this));
            //    }
            //}

            this.CollectProps(this, ref data);

            dataList.Add(data);
            return dataList;
        }

        protected void ReturnData(InstructionSet instructionSet)
        {
            //Tell children to return data
            foreach(Agent child in ChildAgents)
                CreateAndEnqueueInstuction(Agent.BaseInstuctionsMethods.ReturnData.ToString(), "Test", child);

            CreateAndEnqueueInstuction(Agent.BaseInstuctionsMethods.ReceiveData.ToString(), GetData(), instructionSet.SourceAgent);
        }

        protected void SaveChildData(InstructionSet instructionSet)
        {
            var data = instructionSet.ObjectToProcess as List<Dictionary<string, object>>;
            List<Dictionary<string, object>> agentData = data ?? throw new InvalidCastException(
                this.Name + " failed to Cast Dictionary<string, object> on Instruction.ObjectToProcess");

            allChildData.AddRange(agentData);
        }

        protected Boolean CheckAllChildrenResponded()
        {
            bool allChildrenAnswered = true;
            foreach (Agent child in ChildAgents)
            {
                bool childAnswered = false;
                foreach (Dictionary<string, object> dict in allChildData)
                    if (dict.ContainsValue(child.AgentId))
                    {
                        childAnswered = true;
                        break;
                    }
                if (!childAnswered)
                {
                    allChildrenAnswered = false;
                    break;
                }
            }
            return allChildrenAnswered;
        }

        protected void ReceiveData(InstructionSet instructionSet)
        {
            SaveChildData(instructionSet);

            if (CheckAllChildrenResponded())
            {
                CreateAndEnqueueInstuction(Agent.BaseInstuctionsMethods.ReceiveData.ToString(),
                    allChildData, this.Creator);
                // Delete data
                allChildData = null;
            }
        }
    }
}