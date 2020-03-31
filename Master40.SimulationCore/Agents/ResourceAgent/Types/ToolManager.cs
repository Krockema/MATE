using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.DataModel;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public class ToolManager
    {
        private List<M_ResourceSetup> _resourceSetups { get; set; } = new List<M_ResourceSetup>();
        public EquippedResourceTool _equippedResourceTool { get; private set; }  = new EquippedResourceTool();

        public ToolManager(List<M_ResourceSetup> resourceSetups)
        {
            _resourceSetups = resourceSetups;
        }

        internal void Mount(M_ResourceCapability resourceCapability)
        {
            if (!AlreadyEquipped(resourceCapability))
            {
                _equippedResourceTool.Mount(resourceCapability.ResourceSetups.First().ChildResource);
            }
        }

        internal bool AlreadyEquipped(M_ResourceCapability requiredResourceTool)
        {
            return _equippedResourceTool.IsSet(resourceTool: requiredResourceTool);
        }

        internal M_ResourceSetup GetSetupByTool(M_ResourceCapability resourceCapability)
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

        internal object GetToolName()
        {
            string toolName = "was not set";
            if (_equippedResourceTool.ResourceTool != null)
                toolName = _equippedResourceTool.ResourceTool.Name;
            return toolName;
        }
    }
}
