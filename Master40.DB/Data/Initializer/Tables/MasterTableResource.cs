using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableResource
    {

        internal Dictionary<string, List<M_Resource>> Resources = new Dictionary<string, List<M_Resource>>();
        private readonly MasterTableResourceCapability _capability;

        public MasterTableResource(MasterTableResourceCapability capability)
        {
            _capability = capability;
        }

        internal void CreateModel(int sawResource, int drillResource, int assemblyResource)
        {
           CreateResourceGroup(sawResource, _capability.CUTTING.Name, 10);
           CreateResourceGroup(drillResource, _capability.DRILLING.Name, 15);
           CreateResourceGroup(assemblyResource, _capability.ASSEMBLING.Name, 20);
        }

        private void CreateResourceGroup(int numberOfResources, string name,int setupTime)
        {
            List<M_Resource> resourceGroup = new List<M_Resource>();
            for (int i = 0; i < numberOfResources; i++)
            {
                resourceGroup.Add(CreateNewResource(name, i, setupTime));
            }
            Resources.Add(name, resourceGroup);
        }

        private M_Resource CreateNewResource(string resourceName, int number, int setupTime)
        {
            return new M_Resource() { Name = resourceName + " " + number, Capacity = setupTime };
        }
        
        //TODO Konfiguration Anzahl Ressource
        internal void InitSmall(MasterDBContext context)
        {
            CreateModel(1, 1, 1);
            SaveToDB(context);
        }
        internal void InitMedium(MasterDBContext context)
        {
            CreateModel(2, 1, 2);
            SaveToDB(context);
        }

        internal void InitLarge(MasterDBContext context)
        {
            CreateModel(4, 2, 4);
            SaveToDB(context);
        }
        private void SaveToDB(MasterDBContext context)
        {
            foreach (var item in Resources)
            {
                context.Resources.AddRange(entities: item.Value);
            }
            context.SaveChanges();
        }
    }
}