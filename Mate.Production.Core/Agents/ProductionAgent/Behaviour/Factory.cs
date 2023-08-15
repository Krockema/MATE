using Mate.DataCore.Nominal;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Types;
using System;
namespace Mate.Production.Core.Agents.ProductionAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType)
        {
            IBehaviour behaviour;
            switch (simType)
            {
                case SimulationType.Default:
                    behaviour = Default();
                    break;
                default:
                    behaviour = Default();
                    break;
            }

            return behaviour;

        }

        private static IBehaviour Default()
        {

            return new Default();
        }

        /// <summary>
        /// Create the StartConditionMessage which is send to Hub and ResourceAgent
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static UpdateStartConditionRecord GetStartCondition(this OperationRecord operation, DateTime customerDue)
        {
            return new UpdateStartConditionRecord(OperationKey: operation.Key
                , CustomerDue: customerDue
                , PreCondition: operation.StartCondition.PreCondition
                , ArticlesProvided: operation.StartCondition.ArticlesProvided);
        }

    }
}
