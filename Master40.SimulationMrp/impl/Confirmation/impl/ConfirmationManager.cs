using Master40.DB.Data.Helper.Types;

namespace Master40.SimulationMrp.impl.Confirmation.impl
{
    public class ConfirmationManager : IConfirmationManager
    {
        public void CreateConfirmations(SimulationInterval simulationInterval)
        {
            ConfirmationCreator.CreateConfirmations(simulationInterval);
        }

        public void ApplyConfirmations()
        {
            ConfirmationAppliance.ApplyConfirmations();
        }
    }
}