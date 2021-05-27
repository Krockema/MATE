using System.Collections.Generic;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Helper;
using Mate.DataCore.DataModel;

namespace Mate.DataCore.Data.DynamicInitializer.Tables
{
    public class MasterTableResourceCapability
    {
        public List<M_ResourceCapability> Capabilities;
        public List<M_ResourceCapability> ParentCapabilities;
        internal MasterTableResourceCapability()
        {
            Capabilities = new List<M_ResourceCapability>();
        }

        internal M_ResourceCapability[] CreateCapabilities(MateDb context, List<ResourceProperty> resourceProperties)
        {
            for (var i = 0; i < resourceProperties.Count; i++)
            {
                Capabilities.Add(new M_ResourceCapability{Name = "" + AlphabeticNumbering.GetAlphabeticNumbering(i) + " Capability"});
            }

            context.ResourceCapabilities.AddRange(Capabilities);
            context.SaveChanges();
            ParentCapabilities = new List<M_ResourceCapability>(Capabilities);

            for (var i = 0; i < resourceProperties.Count; i++)
            {
                CreateToolingCapabilities(context, Capabilities[i], resourceProperties[i]);
            }

            return Capabilities.ToArray();
        }

        private void CreateToolingCapabilities(MateDb context, M_ResourceCapability parent, ResourceProperty resource)
        {
            var newCapas = new List<M_ResourceCapability>();
            parent.ChildResourceCapabilities = new List<M_ResourceCapability>();
            for (int i = 1; i <= resource.ToolCount; i++)
            {
                var capability = new M_ResourceCapability
                {
                    Name = parent.Name + " Tool Nr " + i,
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