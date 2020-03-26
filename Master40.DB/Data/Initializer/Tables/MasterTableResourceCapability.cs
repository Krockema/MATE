using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableResourceCapability
    {
        internal M_ResourceCapability CUTTING;
        internal M_ResourceCapability DRILLING;
        internal M_ResourceCapability ASSEMBLING;
        internal List<M_ResourceCapability> Capabilities;

        internal MasterTableResourceCapability()
        {
            Capabilities = new List<M_ResourceCapability>();
            CUTTING = new M_ResourceCapability { Name = "Cutting" };
            DRILLING = new M_ResourceCapability { Name = "Drilling" };
            ASSEMBLING = new M_ResourceCapability { Name = "Assembling" };
        }

        internal M_ResourceCapability[] InitBasicCapabilities(MasterDBContext context)
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

        internal M_ResourceCapability[] CreateToolingCapabilities(MasterDBContext context
            , int numberOfSawTools
            , int numberOfDrillingTools
            , int numberOfAssemblingTools)
        {
            CreateToolingCapabilities(context, CUTTING, numberOfSawTools);
            CreateToolingCapabilities(context, DRILLING, numberOfDrillingTools);
            CreateToolingCapabilities(context, ASSEMBLING, numberOfAssemblingTools);
            return Capabilities.ToArray();
        }

        private void CreateToolingCapabilities(MasterDBContext context, M_ResourceCapability parent, int amount)
        {
            var newCapas = new List<M_ResourceCapability>();
            parent.ChildResourceCapabilities = new List<M_ResourceCapability>();
            for (int i = 0; i < amount; i++)
            {
                var capability = new M_ResourceCapability
                {
                    Name = parent.Name + " Tool Nr. " + i,
                    ParentResourceCapabilityId = parent.Id
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