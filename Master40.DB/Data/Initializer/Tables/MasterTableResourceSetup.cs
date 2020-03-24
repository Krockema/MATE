using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    public class MasterTableResourceSetup
    {
        internal List<M_ResourceSetup> ResourceSetups = new List<M_ResourceSetup>();

        private M_ResourceSetup createResourceSetup(string name
            , int resourceId
            , int resourceToolId
            , int resourceCapabilityId
            , int setupTime)
        {
            return new M_ResourceSetup
            {
                Name = name,
                ResourceId = resourceId,
                ResourceToolId = resourceToolId,
                ResourceCapabilityId = resourceCapabilityId,
                SetupTime = setupTime
            };
        }

        internal M_ResourceSetup[] InitCustom(
                                          MasterDBContext ctx      
                                          , MasterTableResource resources
                                          , MasterTableResourceTool resourceTools
                                          , MasterTableResourceCapability resourceCapabilities)
        {

            foreach (var resourceDict in resources.Resources)
            {
                foreach (var toolDict in resourceTools.ResourceTools.Where(x => x.Key.Equals(resourceDict.Key)))
                {
                    foreach (var tool in toolDict.Value)
                    {
                        foreach (var resource in resourceDict.Value)
                        {
                            ResourceSetups.Add(createResourceSetup(
                                name: resource.Name + " " + tool.Name,
                                resourceId: resource.Id,
                                resourceToolId: tool.Id,
                                resourceCapabilityId: resourceCapabilities.Capabilities.Single(x => x.Name.Equals(resourceDict.Key)).Id,
                                setupTime: resource.Capacity
                            ));


                        }
                    }
                }
            }
            ctx.ResourceSetups.AddRange(ResourceSetups);
            ctx.SaveChanges();
            return ResourceSetups.ToArray();
        }

    }
}