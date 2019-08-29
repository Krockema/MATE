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
        private EquippedResourceTool _equippedResourceTool { get; set; } =new EquippedResourceTool();

        public ToolManager(List<M_ResourceSetup> resourceSetups)
        {
            _resourceSetups = resourceSetups;
        }

        internal void Mount(M_ResourceTool requiredResourceTool)
        {
            if (!AlreadyEquipped(requiredResourceTool))
            {
                _equippedResourceTool.Mount(requiredResourceTool);
            }
        }

        internal bool AlreadyEquipped(M_ResourceTool requiredResourceTool)
        {
            return _equippedResourceTool.IsSet(resourceTool: requiredResourceTool);
        }

        internal M_ResourceSetup GetSetupByTool(M_ResourceTool resourceTool)
        {
            //TODO Take care if 1 Skill can be done by multiply tools
            var resourceSetup = _resourceSetups.SingleOrDefault(x => x.ResourceToolId == resourceTool.Id);
            return resourceSetup;
        }

        internal List<M_ResourceSetup> GetAllSetups()
        {
            return _resourceSetups;
        }
    }
}
