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
            _resources.TryAdd(resource.Resource.GetValue().GetId(), resource);
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
        /// Get all available Resources as Dictionary<Id, Resource>
        /// </summary>
        /// <returns>ResourceDictionary</returns>
        public static ResourceDictionary GetResources(IDbMasterDataCache masterDataCache)
        {
            var resources = new ResourceDictionary();
            foreach (var resourceSkill in masterDataCache.M_ResourceSkillGetAll())
            {
                resources.Add(resourceSkill.GetId(),
                    masterDataCache.ResourcesGetAllBySkillId(resourceSkill.GetId()));
            }
            return resources;
        }
    }
}