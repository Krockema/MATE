using System;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.Nominal.Model;

namespace Master40.DB.Data.Initializer
{
    public static class ResourceInitializer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">Database context, the results are written to</param>
        /// <param name="resourceModelSize">Determines the number of resources for reach resource group </param>
        /// <param name="setupModelSize">Determines the number of possible setups for each resource group </param>
        /// <param name="operatorsModelSize">Add Operator that is required for setup</param>
        /// <param name="numberOfWorkersForProcessing">Add a required resource for Processing</param>
        /// <param name="secondResource">Add a second primary resource that is required for setup and processing</param>
        /// <returns></returns>
        public static MasterTableResourceCapability MasterTableResourceCapability(MasterDBContext context,
            ModelSize resourceModelSize, ModelSize setupModelSize, ModelSize operatorsModelSize,
            int numberOfWorkersForProcessing = 0, bool secondResource = false)
        {
            // requires Tools and Resources
            var resourceCapabilities = new MasterTableResourceCapability();
            resourceCapabilities.InitBasicCapabilities(context);
            switch (setupModelSize)
            {
                case ModelSize.Small:
                    resourceCapabilities.CreateToolingCapabilities(context, 2, 2, 2);
                    break;
                case ModelSize.Medium:
                    resourceCapabilities.CreateToolingCapabilities(context, 4, 4, 7);
                    break;
                case ModelSize.Large:
                    resourceCapabilities.CreateToolingCapabilities(context, 8, 8, 14);
                    break;
                case ModelSize.TestModel:
                    resourceCapabilities.CreateToolingCapabilities(context, 4, 4, 7);
                    break;
                default: throw new ArgumentException();
            }

            var resources = new MasterTableResource(resourceCapabilities);
            switch (resourceModelSize)
            {
                case ModelSize.Small:
                    resources.InitSmall(context);
                    break;
                case ModelSize.Medium:
                    resources.InitMedium(context);

                    break;
                case ModelSize.Large:
                    resources.InitLarge(context);
                    break;
                case ModelSize.XLarge:
                    resources.InitXLarge(context);
                    break;
                case ModelSize.TestModel:
                    resources.InitMediumTest(context);
                    break;
                default: throw new ArgumentException();
            }

            var operatorModel = new int[] { 0, 0, 0 };
            switch (operatorsModelSize)
            {
                case ModelSize.None:
                    operatorModel = new int[] { 0, 0, 0 };
                    break;
                case ModelSize.Small:
                    operatorModel = new int[] { 1, 0, 1 };
                    break;
                case ModelSize.Medium:
                    operatorModel = new int[] { 1, 1, 1 };
                    break;
                default: throw new ArgumentException();
            }

            resources.CreateResourceTools(setupTimeCutting: 10, setupTimeDrilling: 15, setupTimeAssembling: 20, operatorModel,
                numberOfWorkers: numberOfWorkersForProcessing, secondResource);
            resources.SaveToDB(context);
            return resourceCapabilities;
        }
    }
    
}