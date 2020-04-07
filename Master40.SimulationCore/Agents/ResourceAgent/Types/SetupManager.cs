using Master40.DB.DataModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static FSetupDefinitions;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class SetupManager
    {
        private List<M_ResourceSetup> _resourceSetups { get; set; } = new List<M_ResourceSetup>();
        private SetupInUse _setupInUse { get; set; } 
        public SetupManager(List<M_ResourceSetup> resourceSetups)
        {
            _resourceSetups = resourceSetups;
            _setupInUse = new SetupInUse();
        }

        internal void Mount(M_ResourceCapability resourceCapability)
        {
            var setup = _resourceSetups.Single(x => x.ResourceCapability.Id == resourceCapability.Id);

            if (!AlreadyEquipped(setup))
            {  
                _setupInUse.Mount(setup);
            }

        }

        public M_ResourceCapability GetCurrentUsedCapability()
        {
            return _setupInUse.ResourceSetup?.ResourceCapability;
        }

        internal bool AlreadyEquipped(M_ResourceSetup setup)
        {
            if(_setupInUse.ResourceSetup == null)
                return false;
            return _setupInUse.ResourceSetup.Id == setup.Id;
        }

        internal M_ResourceSetup GetSetupByCapability(M_ResourceCapability resourceCapability)
        {
            //TODO Take care if 1 Capability can be done by multiply tools
            var resourceSetup = _resourceSetups.SingleOrDefault(x => x.ResourceCapabilityId == resourceCapability.Id);
            return resourceSetup;
        }

        internal long GetSetupDurationByTool(M_ResourceCapability resourceCapability)
        {
            //TODO Take care if 1 Capability can be done by multiply tools
            var setupTime = _resourceSetups.SingleOrDefault(x => x.ResourceCapabilityId == resourceCapability.Id).SetupTime;
            return setupTime;
        }

        internal List<M_ResourceSetup> GetAllSetups()
        {
            return _resourceSetups;
        }
        internal List<M_ResourceCapability> GetAllCapabilities()
        {
            return _resourceSetups.Select(x => x.ResourceCapability).ToList();
        }

        internal object GetSetupName()
        {
            string setupName = "was not set";
            if (_setupInUse.ResourceSetup != null)
                setupName = _setupInUse.ResourceSetup.Name;
            return setupName;
        }

        internal int CurrentSetupId => _setupInUse.SetupId();
        

    }
}
