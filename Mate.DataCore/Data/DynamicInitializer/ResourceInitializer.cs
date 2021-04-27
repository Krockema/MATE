using System.Collections.Generic;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.DynamicInitializer.Tables;

namespace Mate.DataCore.Data.DynamicInitializer
{
    public class ResourceInitializer
    {
        public static MasterTableResourceCapability Initialize(MateDb context, List<ResourceProperty> resourceProperties)
        {
            var resourceCapabilities = new MasterTableResourceCapability();
            resourceCapabilities.CreateCapabilities(context, resourceProperties);

            var resources = new MasterTableResource(resourceCapabilities);
            resources.CreateModel(resourceProperties);
            resources.CreateResourceTools(resourceProperties);

            resources.SaveToDB(context);
            return resourceCapabilities;
        }
    }
}