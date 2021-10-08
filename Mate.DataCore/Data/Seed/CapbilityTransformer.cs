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
        public static MasterTableResourceCapability Transform(MateDb mateDb, ResourceConfig resourceConfig) { 
            
            var resourceProperties = new List<ResourceProperty>();

            for(var i = 0; i < resourceConfig.ResourceGroupList.Count(); i++)
            {
                //TODO: take SetupTime from config after SEED Fix
                resourceProperties.Add(new ResourceProperty { 
                    Name = resourceConfig.ResourceGroupList[i].Name, 
                    ToolCount = resourceConfig.GetToolsFor(i).Count, 
                    ResourceCount = (int)resourceConfig.ResourceGroupList[i].ResourceQuantity, 
                    SetupTime = (int)resourceConfig.GetMeanSetupDurationFor(i, 0).TotalMinutes,
                    OperatorCount = 2, 
                    IsBatchAble = false
                });
            
            }

            var masterTableResourceCapability = ResourceInitializer.Initialize(mateDb, resourceProperties);

            return masterTableResourceCapability;
        }
    }
}
