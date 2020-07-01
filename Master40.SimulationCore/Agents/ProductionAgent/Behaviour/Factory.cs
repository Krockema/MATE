using Master40.DB.Nominal;
using Master40.SimulationCore.Types;
using static FOperations;
using static FUpdateStartConditions;

namespace Master40.SimulationCore.Agents.ProductionAgent.Behaviour
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
        public static FUpdateStartCondition GetStartCondition(this FOperation operation, long customerDue)
        {
            return new FUpdateStartCondition(operationKey: operation.Key
                , customerDue: customerDue
                , preCondition: operation.StartConditions.PreCondition
                , articlesProvided: operation.StartConditions.ArticlesProvided);
        }

    }
}
