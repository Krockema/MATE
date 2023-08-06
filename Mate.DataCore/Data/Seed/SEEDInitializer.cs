using System;
using System.Collections.Generic;
using Seed;
using Seed.Parameter;
using Seed.Parameter.TransitionMatrix;
using Seed.Parameter.Operation;
using Seed.Generator.Material;
using Seed.Distributions;
using Seed.Generator.Operation;
using Mate.DataCore.Data.Context;
using System.Linq;

namespace Mate.DataCore.Data.Seed
{
    public class SeedInitializer
    {
        public void GenerateTestData(string mateDbName, int machineCount, int toolCount
                                    , int seed, double reuseRatio, double complexityRatio, double organizationalDegree
                                    , int numberOfSalesMaterials, int verticalIntegration, int lambda = 2)
        {

            //Generate Config
            var seedConfig = new Configuration();
            var materialConfig = new MaterialConfig()
            {
                StructureParameter = new StructureParameter() {
                    ComplexityRatio = complexityRatio, // 1.9,
                    ReuseRatio = reuseRatio, // 1.3,
                    NumberOfSalesMaterials = numberOfSalesMaterials,
                    VerticalIntegration = verticalIntegration,
                    Seed = seed // 7
                },
                TransitionMatrixParameter = new TransitionMatrixParameter() {
                    Lambda = lambda,
                    OrganizationalDegree = organizationalDegree // 0.7
                }
            };

            seedConfig.WithOption(materialConfig);



            var rsSaw = new ResourceGroup("Saw")
               .WithResourceuQuantity(machineCount)
               .WithTools(GenerateTools(toolCount));

            var rsDrill = new ResourceGroup("Drill")
                .WithResourceuQuantity(machineCount)
                .WithTools(GenerateTools(toolCount));

            var rsAssembly = new ResourceGroup("Assembly")
                .WithResourceuQuantity(machineCount)
                .WithTools(GenerateTools(toolCount));

            var rsQuality = new ResourceGroup("Quality")
                .WithResourceuQuantity(machineCount)
                .WithTools(GenerateTools(toolCount));

            var resourceConfig = new ResourceConfig().WithResourceGroup(new List<ResourceGroup> { rsSaw, rsDrill, rsAssembly, rsQuality })
                                                .WithDefaultOperationsDurationMean(TimeSpan.FromMinutes(10))
                                                .WithDefaultOperationsDurationVariance(0.20)
                                                .WithDefaultOperationsAmountMean(8)
                                                .WithDefaultOperationsAmountVariance(0.20);

            seedConfig.WithOption(resourceConfig);

            // Generator
            var materials = MaterialGenerator.WithConfiguration(materialConfig)
                                             .Generate();

            var randomizer = new RandomizerBase(materialConfig.StructureParameter.Seed);
            var randomizerCollection = RandomizerCollection.WithTransition(randomizer)
                                                .WithOperationDuration(new RandomizerLogNormal(randomizer.Next(int.MaxValue)))
                                                .WithOperationAmount(new RandomizerBinominial(randomizer.Next(int.MaxValue)))
                                                .Build();


            var transitionMatrix = TransitionMatrixGenerator.WithConfiguration(seedConfig).Generate();

            var operationDistributor = OperationDistributor.WithTransitionMatrix(transitionMatrix)
                                                            .WithRandomizerCollection(randomizerCollection)
                                                            .WithResourceConfig(resourceConfig)
                                                            .WithMaterials(materials)
                                                            .Build();

            var operationGenerator = OperationGenerator.WithOperationDistributor(operationDistributor)
                                                       .WithMaterials(materials.NodesInUse.Where(x => x.IncomingEdgeIds.Any()).ToArray())
                                                       .Generate();


            //Initilize DB

            MateDb mateDb = Dbms.GetMateDataBase(dbName: mateDbName).DbContext;
            mateDb.Database.EnsureDeleted();
            mateDb.Database.EnsureCreated();

            var masterTableCapabilities = CapbilityTransformer.Transform(mateDb, resourceConfig);

            MaterialTransformer.Transform(mateDb, materials, masterTableCapabilities);

        }


        private List<ResourceTool> GenerateTools(int amount)
        {
            var listOfTools = new List<ResourceTool>();
            var counter = 2;
            

            for (int i = 0; i < amount; i++)
            {

                var nameOfTool = "Blade" + counter.ToString() + "mm";
                listOfTools.Add(new ResourceTool(nameOfTool).WithOperationDurationAverage(TimeSpan.FromMinutes(30)).WithOperationDurationVariance(0));
                counter += 2;
            }


            return listOfTools;

        }
    }

}