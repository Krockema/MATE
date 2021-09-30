using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.DynamicInitializer;
using Mate.DataCore.Data.DynamicInitializer.Tables;
using Mate.DataCore.DataModel;
using Seed.Parameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.DataCore.Data.Seed
{
    public class CapbilityTransformer
    {
        public static List<M_ResourceCapability> Transform(MateDb mateDb, ResourceConfig resourceConfig) { 
            
            var resourceProperties = new List<ResourceProperty>();
            
            foreach(var group in resourceConfig.ResourceGroupList)
            { 
                //TODO: take SetupTime from config after SEED Fix
                resourceProperties.Add(new ResourceProperty { Name = group.Name, ToolCount = group.Tools.Count, ResourceCount = (int)group.ResourceQuantity, SetupTime = 10, OperatorCount = 0, IsBatchAble = false });
            }

            var capabilities = ResourceInitializer.Initialize(mateDb, resourceProperties);
            
            return capabilities.Capabilities;
        }
    }
}
