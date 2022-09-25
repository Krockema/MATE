using System;
using System.Collections.Generic;
using Xunit;

using Seed;
using Seed.Parameter;
using Seed.Parameter.TransitionMatrix;
using Seed.Parameter.Operation;
using Seed.Generator.Material;
using Seed.Distributions;
using Seed.Generator.Operation;
using Mate.DataCore.Data.Seed;
using Mate.DataCore.Data.Context;
using Mate.DataCore;
using System.Linq;

namespace Mate.Test.SimulationEnvironment
{
    public class SeedInitializer : IDisposable
    {

        private readonly string TestMateDb = "Test" + DataBaseConfiguration.MateDb;

        public void Dispose()
        {
        }

        public void GenerateTestData(string mateDbName)
        {
            
            //Generate Config
            var seedConfig = new Configuration();
            var materialConfig = new MaterialConfig()
            {
                StructureParameter = new StructureParameter() {
                    ComplexityRatio = 1.9,
                    ReuseRatio = 1.3,
                    NumberOfSalesMaterials = 100,
                    VerticalIntegration = 4,
                    // Seed = 3
                },
                TransitionMatrixParameter = new TransitionMatrixParameter() { 
                    Lambda = 2,
                    OrganizationalDegree = 0.7 
                }
            };

            seedConfig.WithOption(materialConfig);



            var rsSaw = new ResourceGroup("Saw")
               .WithResourceuQuantity(4)
               .WithTools(new List<ResourceTool> {
                    new ResourceTool("Blade 4mm"),
                    new ResourceTool("Blade 6mm"),
                    new ResourceTool("Blade 8mm"),
                    new ResourceTool("Blade 10mm"),
                    new ResourceTool("Blade 12mm"),
                    new ResourceTool("Blade 14mm")
               });

            var rsDrill = new ResourceGroup("Drill")
                .WithResourceuQuantity(4)
                .WithTools(new List<ResourceTool> {
                    new ResourceTool("Blade 4mm"),
                    new ResourceTool("Blade 6mm"),
                    new ResourceTool("Blade 8mm"),
                    new ResourceTool("Blade 10mm"),
                    new ResourceTool("Blade 12mm"),
                    new ResourceTool("Blade 14mm")
                });

            var rsAssembly = new ResourceGroup("Assembly")
                .WithResourceuQuantity(4)
                .WithTools(new List<ResourceTool> {
                    new ResourceTool("Blade 4mm"),
                    new ResourceTool("Blade 6mm"),
                    new ResourceTool("Blade 8mm"),
                    new ResourceTool("Blade 10mm"),
                    new ResourceTool("Blade 12mm"),
                    new ResourceTool("Blade 14mm")
                });

            var rsQuality = new ResourceGroup("Quality")
                .WithResourceuQuantity(4)
                .WithTools(new List<ResourceTool> {
                    new ResourceTool("Blade 4mm"),
                    new ResourceTool("Blade 6mm"),
                    new ResourceTool("Blade 8mm"),
                    new ResourceTool("Blade 10mm"),
                    new ResourceTool("Blade 12mm"),
                    new ResourceTool("Blade 14mm")
                });

            var resourceConfig = new ResourceConfig().WithResourceGroup(new List<ResourceGroup> { rsSaw, rsDrill, rsAssembly, rsQuality})
                                                .WithDefaultOperationsDurationMean(TimeSpan.FromMinutes(10))
                                                .WithDefaultOperationsDurationVariance(0.20)
                                                .WithDefaultSetupDurationMean(TimeSpan.FromMinutes(30))
                                                .WithDefaultOperationsAmountMean(5)
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

    }
}
