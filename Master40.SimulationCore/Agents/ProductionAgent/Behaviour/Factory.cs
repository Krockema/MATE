using Master40.DB.Enums;
using Master40.SimulationCore.Types;
using static FOperations;
using static FUpdateStartConditions;

namespace Master40.SimulationCore.Agents.ProductionAgent.Behaviour
{
    public static class Factory
    {
        public static IBehaviour Get(SimulationType simType)
        {
            switch (simType)
            {
                default:
                    return Default();
            }

        }

        private static IBehaviour Default()
        {

            return new Default();
        }


        public static FUpdateStartCondition GetStartCondition(this FOperation operation)
        {
            return new FUpdateStartCondition(operationKey: operation.Key
                , preCondition: operation.StartConditions.PreCondition
                , articlesProvided: operation.StartConditions.ArticlesProvided);
        }

    }
}
