using Master40.DB.Data.WrappersForPrimitives;
using System;
using System.Collections.Generic;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.DbCache;
using Zpp.Mrp.MachineManagement;

namespace Zpp.Simulation.Agents.JobDistributor.Types
{
    public class ResourceManager 
    {
        private readonly Dictionary<Id, ResourceDetails> _resources = new Dictionary<Id, ResourceDetails>();
        
        public int Count => _resources.Count;

        public void AddResource(ResourceDetails resource)
        {
            _resources.TryAdd(resource.Machine.GetValue().GetId(), resource);
        }

        public ResourceDetails GetResourceRefById(Id id)
        {
            if (_resources.TryGetValue(id, out ResourceDetails resourceDetails))
            {
                return resourceDetails;
            }
            throw new Exception($"No resource for {id.GetValue()} found");
        }

        /// <summary>
        /// Get all available Resources as Dictionary<Id, Machine>
        /// </summary>
        /// <returns>ResourceDictionary</returns>
        public static ResourceDictionary GetResources(IDbMasterDataCache masterDataCache)
        {
            var resources = new ResourceDictionary();
            foreach (var machineGroup in masterDataCache.M_MachineGroupGetAll())
            {
                resources.Add(machineGroup.GetId(),
                    masterDataCache.M_MachineGetAllByMachineGroupId(machineGroup.GetId()));
            }
            return resources;
        }
    }
}
