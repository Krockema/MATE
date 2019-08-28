using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Master40.Agents.Agents.DataTransformation;
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
        // Creator is not always the actual creator
        internal Agent Creator { get; set; }
        // Agent who created this agent, used for data aggregation
        internal Agent Parent { get; }
        internal List<Agent> ChildAgents { get; set; }
        public string Name { get; set; }
        public bool DebugThis { get; set; }
        internal Status Status { get; set; }
        public Queue<InstructionSet> InstructionQueue { get; set; }
        protected List<Dictionary<string, object>> allChildData = new List<Dictionary<string, object>>();

        protected Agent(Agent creator, Agent parent, string name, bool debug)
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

            this.Parent = parent;

            DebugMessage(" created by " + creatorsName + ", GUID: " + AgentId);
        }

        public enum BaseInstuctionsMethods
        {
            ReturnData,
            ReceiveData,
            InjectData
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

        private List<Dictionary<string, object>> GetData()
        {
            List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("AgentId", AgentId);
            data.Add("AgentType", this.GetType());
            
            DataCollectionHelper.CollectProps(this, ref data, this.GetType().Name + ".");

            dataList.Add(data);
            return dataList;
        }

        protected void ReturnData(InstructionSet instructionSet)
        {
            //Tell children to return data
            foreach(Agent child in ChildAgents)
                CreateAndEnqueueInstuction(Agent.BaseInstuctionsMethods.ReturnData.ToString(), "ReturnData", child);

            //TODO: Move into ReceiveData?
            CreateAndEnqueueInstuction(Agent.BaseInstuctionsMethods.ReceiveData.ToString(), GetData(), this);
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
                    if (dict.Contains(new KeyValuePair<string, object>("AgentId", child.AgentId)))
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
                    allChildData, this.Parent);
                // Delete data
                allChildData = new List<Dictionary<string, object>>();
            }
        }

        protected void InjectData(InstructionSet instructionSet)
        {
            var data = instructionSet.ObjectToProcess as List<Dictionary<string, object>>;
            List<Dictionary<string, object>> agentData = data ?? throw new InvalidCastException(
                this.Name + " failed to Cast Dictionary<string, object> on Instruction.ObjectToProcess");

            List<AgentPropertyBase> propTree = AgentPropertyManager.GetPropertiesByAgentName(this.GetType().Name);
            foreach(AgentPropertyBase prop in propTree)
            {
                InjectDataRecursion(prop, this, this.GetType().Name, agentData);
            }

            foreach (Agent child in ChildAgents)
                CreateAndEnqueueInstuction(Agent.BaseInstuctionsMethods.InjectData.ToString(), data, child);
        }

        private void InjectDataRecursion(AgentPropertyBase prop, object obj, string propPath, List<Dictionary<string, object>> data)
        {
            if(prop.IsNode())
            {
                AgentPropertyNode propNode = (AgentPropertyNode)prop;
                PropertyInfo propInfo = obj.GetType().GetProperty(prop.GetPropertyName(), DataCollectionHelper.flags);
                object nodeObject = propInfo.GetValue(obj);
                propPath = propPath + "." + prop.GetPropertyName();
                // TODO: for now only process first element of list
                if (nodeObject is IList && ((IList)nodeObject).Count > 0)
                {
                    nodeObject = ((IList)nodeObject)[0];
                }
                List<AgentPropertyNode> propNodes = propNode.GetPropertyNodes();
                foreach(AgentPropertyNode node in propNodes)
                {
                    InjectDataRecursion(node, nodeObject, propPath, data);
                }

                List<AgentProperty> idProps = propNode.GetDirectProperties(PropertyType.Id);
                List<AgentProperty> retransformProps = propNode.GetDirectProperties(PropertyType.Retransform);
                if (idProps.Count == 0 || retransformProps.Count == 0)
                    return;

                Dictionary<string, object> idValues = GetIdPropValues(idProps, nodeObject, propPath);
                
                Dictionary<string, object> matchingData = FindMatchingDict(idValues, data);

                SetNewPropValues(retransformProps, propPath, matchingData);
            }
            else
            {
                // Ignore, since at least 2 props are needed for Id + Data
            }
        }

        private Dictionary<string, object> GetIdPropValues(List<AgentProperty> idProps, object nodeObject, string propPath)
        {
            List<AgentPropertyBase> idPropsBase = new List<AgentPropertyBase>();
            foreach (AgentProperty idProp in idProps)
            {
                idPropsBase.Add((AgentPropertyBase)idProp);
            }
            Dictionary<string, object> idValues = new Dictionary<string, object>();
            DataCollectionHelper.CollectPropsRecursion(idPropsBase, nodeObject, ref idValues, propPath + ".");
            return idValues;
        }

        private Dictionary<string, object> FindMatchingDict(Dictionary<string, object> idValues, List<Dictionary<string, object>> data)
        {
            foreach (Dictionary<string, object> dataSet in data)
            {
                bool found = true;
                foreach (KeyValuePair<string, object> pair in idValues)
                {
                    object dataValue;
                    if (dataSet.TryGetValue(pair.Key, out dataValue) || (dataValue == pair.Value))
                    {
                        continue;
                    }
                    found = false;
                }
                if (found)
                {
                    return dataSet;
                }
            }
            return new Dictionary<string, object>();
        }

        private void SetNewPropValues(List<AgentProperty> retransformProps, string propPath, Dictionary<string, object> matchingData)
        {
            foreach (AgentProperty retransformProp in retransformProps)
            {
                string currentPropPath = propPath + "." + retransformProp.GetPropertyName();
                string[] propPathArray = currentPropPath.Split(".").Skip(1).ToArray();
                object newValue;
                if (matchingData.TryGetValue(currentPropPath, out newValue))
                {
                    SetNestedPropertyValue(this, propPathArray, newValue);
                }
            }
        }

        private void SetNestedPropertyValue(object obj, string[] propPath, object value)
        {
            PropertyInfo propInfo = obj.GetType().GetProperty(propPath[0], DataCollectionHelper.flags);
            if (propPath.Length == 1)
            {
                // Set value                
                propInfo.SetValue(obj, value);
            }
            else
            {
                // get property
                object childValue = propInfo.GetValue(obj);
                // TODO: for now only process first element of list
                if (childValue is IList && ((IList)childValue).Count > 0)
                {
                    childValue = ((IList)childValue)[0];
                }
                SetNestedPropertyValue(childValue, propPath.Skip(1).ToArray(), value);
            }
        }
    }
}