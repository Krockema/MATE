using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.DirectoryAgent;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Agents.ProductionAgent.Types;
using Master40.SimulationCore.Agents.StorageAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using static FAgentInformations;
using static FArticleProviders;
using static FArticles;
using static FCreateSimulationWorks;
using static FOperations;
using static FProductionResults;
using static FThroughPutTimes;
using static IJobResults;

namespace Master40.SimulationCore.Agents.ProductionAgent.Behaviour
{
    public class Bucket : SimulationCore.Types.Behaviour
    {
        internal Bucket(SimulationType simulationType = SimulationType.None)
            : base(childMaker: null, obj: simulationType)
        {
        }

        /// <summary>
        /// Operation related Hubagents
        /// </summary>
        internal AgentDictionary _hubAgents { get; set; } = new AgentDictionary();
        /// <summary>
        /// Article this Production Agent has to Produce
        /// </summary>
        internal FArticle _articleToProduce { get; set; }
        /// <summary>
        /// Class to supervise operations, supervise operation material handling, articles required by operation, and their relation
        /// </summary>
        internal OperationManager OperationManager { get; set; } = new OperationManager();

        internal ForwardScheduleTimeCalculator _forwardScheduleTimeCalculator { get; set; }
        public override bool Action(object message)
        {
            switch (message)
            {
                //case Production.Instruction.StartProduction msg: StartProductionAgent(fArticle: msg.GetObjectFromMessage); break;
                //case BasicInstruction.ResponseFromDirectory msg: SetHubAgent(hub: msg.GetObjectFromMessage); break;
                //case BasicInstruction.JobForwardEnd msg: AddForwardTime(earliestStartForForwardScheduling: msg.GetObjectFromMessage); break;
                //case BasicInstruction.ProvideArticle msg: ArticleProvided(msg.GetObjectFromMessage); break;
                //case BasicInstruction.WithdrawRequiredArticles msg: WithdrawRequiredArticles(operationKey: msg.GetObjectFromMessage); break;
                //case BasicInstruction.FinishJob msg: FinishJob(msg.GetObjectFromMessage); break;

                default: return false;
            }

            return true;
        }

    }
}
