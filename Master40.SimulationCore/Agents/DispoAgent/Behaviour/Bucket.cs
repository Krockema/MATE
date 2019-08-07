using Akka.Actor;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Helper;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master40.SimulationCore.Agents.DispoAgent.Behaviour
{
    public class Bucket : Default
    {
        internal Bucket(Dictionary<string, object> properties) : base(properties, SimulationType.Bucket) { }

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {

                case Dispo.Instruction.ResponseFromSystemForBom r: ResponseFromSystemForBom((Dispo)agent, r.GetObjectFromMessage); break;
                case BasicInstruction.ResponseFromHub r: base.ResponseFromHub((Dispo)agent, r.GetObjectFromMessage); break;
                case Dispo.Instruction.RequestArticle r: base.RequestArticle((Dispo)agent, r.GetObjectFromMessage); break;
                case Dispo.Instruction.ResponseFromStock r: base.ResponseFromStock((Dispo)agent, r.GetObjectFromMessage); break;
                case Dispo.Instruction.WithdrawMaterialsFromStock r: base.WithdrawMaterial((Dispo)agent); break;
                case Dispo.Instruction.RequestProvided r: base.RequestProvided((Dispo)agent, r.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        private new void ResponseFromSystemForBom(Dispo agent, M_Article article)
        {
            // Update 
            var requestItem = agent.Get<FRequestItem>(Dispo.Properties.REQUEST_ITEM);
            var stockAgent = agent.Get<IActorRef>(Dispo.Properties.STORAGE_AGENT_REF);
            var quantityToProduce = agent.Get<int>(Dispo.Properties.QUANTITY_TO_PRODUCE);
            long dueTime = requestItem.DueTime;

            if (article.WorkSchedules != null)
                dueTime = requestItem.DueTime - article.WorkSchedules.Sum(x => x.Duration); //- Calculations.GetTransitionTimeForWorkSchedules(item.Article.WorkSchedules);


            FRequestItem newItem = requestItem.UpdateCustomerOrderAndDue(requestItem.CustomerOrderId, dueTime, stockAgent)
                                             .UpdateArticle(article);
            agent.Set(Dispo.Properties.REQUEST_ITEM, newItem);

            // Creates a Production Agent for each element that has to be produced
            for (int i = 0; i < quantityToProduce; i++)
            {
                var agentSetup = AgentSetup.Create(agent, ProductionAgent.Behaviour.BehaviourFactory.Get(SimulationType.Bucket));
                var instruction = Guardian.Guardian.Instruction.CreateChild.Create(agentSetup, agent.Guardian);
                agent.Send(instruction);
            }
        }


    }
}
