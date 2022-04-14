using System.Collections.Generic;
using Mate.DataCore.Data.Context;
using Mate.DataCore.DataModel;

namespace Mate.DataCore.Data.Initializer.Tables
{
    public class MasterTableResourceCapability
    {
        public M_ResourceCapability CUTTING;
        public M_ResourceCapability DRILLING;
        public M_ResourceCapability ASSEMBLING;
        public List<M_ResourceCapability> Capabilities;

        internal MasterTableResourceCapability()
        {
            Capabilities = new List<M_ResourceCapability>();
            CUTTING = new M_ResourceCapability { Name = "Cutting" };
            DRILLING = new M_ResourceCapability { Name = "Drilling" };
            ASSEMBLING = new M_ResourceCapability { Name = "Assembling" };
        }

        internal M_ResourceCapability[] InitBasicCapabilities(MateDb context)
        {
            Capabilities = new List<M_ResourceCapability>
            {
                CUTTING,
                DRILLING,
                ASSEMBLING,
            };

            context.ResourceCapabilities.AddRange(Capabilities);
            context.SaveChanges();
            return Capabilities.ToArray();
        }

        internal M_ResourceCapability[] CreateToolingCapabilities(MateDb context
            , int numberOfSawTools
            , int numberOfDrillingTools
            , int numberOfAssemblingTools)
        {
            CreateToolingCapabilities(context, CUTTING, numberOfSawTools);
            CreateToolingCapabilities(context, DRILLING, numberOfDrillingTools);
            CreateToolingCapabilities(context, ASSEMBLING, numberOfAssemblingTools);
            return Capabilities.ToArray();
        }

        private void CreateToolingCapabilities(MateDb context, M_ResourceCapability parent, int amount)
        {
            var newCapas = new List<M_ResourceCapability>();
            parent.ChildResourceCapabilities = new List<M_ResourceCapability>();
            for (int i = 0; i < amount; i++)
            {
                var capability = new M_ResourceCapability
                {
                    Name = parent.Name + " Tool Nr " + i,
                    ParentResourceCapabilityId = parent.Id,
                    IsBatchAble = false,
                    BatchSize = 1
                    
                };

                Capabilities.Add(capability);
                newCapas.Add(capability);
                parent.ChildResourceCapabilities.Add(capability);
            }
            
            context.ResourceCapabilities.AddRange(newCapas);
            context.SaveChanges();
        }

    }
}