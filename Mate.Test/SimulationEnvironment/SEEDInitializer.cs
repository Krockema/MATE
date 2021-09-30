using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace Mate.Test.SimulationEnvironment
{
    public class SEEDInitializer
    {
        [Fact]
        public void InitiateSeed()
        {
            var materials = new Materials();
            //Initialize MaterialConfig
            var seedConfig = new Configuration();
            var materialConfig = new MaterialConfig()
            {
                StructureParameter = new StructureParameter() { ComplexityRatio = 4, ReuseRatio = 2, NumberOfSalesMaterials = 8, VerticalIntegration = 4},
                TransitionMatrixParameter = new TransitionMatrixParameter() { Lambda = 2, OrganizationalDegree = 0.15 }
            };
            seedConfig.WithOption(materialConfig);

            var rsSaw = new ResourceGroup("Saw")
               .WithResourceuQuantity(2)
               .WithDefaultSetupDurationMean(TimeSpan.FromMinutes(5))
               .WithTools(new List<ResourceTool> {
                    new ResourceTool("Blade 4mm").WithOperationDurationAverage(TimeSpan.FromMinutes(6)).WithOperationDurationVariance(0.20),
                    new ResourceTool("Blade 6mm").WithOperationDurationAverage(TimeSpan.FromMinutes(8)).WithOperationDurationVariance(0.20),
                    new ResourceTool("Blade 8mm").WithOperationDurationAverage(TimeSpan.FromMinutes(10)).WithOperationDurationVariance(0.20)
               });

            var rsDrill = new ResourceGroup("Drill")
                .WithResourceuQuantity(1)
                .WithDefaultSetupDurationMean(TimeSpan.FromMinutes(5))
                .WithDefaultOperationDurationMean(TimeSpan.FromMinutes(5))
                .WithDefaultOperationDurationVariance(0.20)
                .WithTools(new List<ResourceTool> {
                    new ResourceTool("Head 10mm"),
                    new ResourceTool("Head 15mm"),
                });

            var resourceConfig = new ResourceConfig().WithResourceGroup(new List<ResourceGroup> { rsSaw, rsDrill})
                                               .WithDefaultOperationsDurationMean(TimeSpan.FromSeconds(300))
                                               .WithDefaultOperationsDurationVariance(0.20)
                                               .WithDefaultOperationsAmountMean(4)
                                               .WithDefaultOperationsAmountVariance(0.20);

            seedConfig.WithOption(resourceConfig);

            // Generator
            materials = MaterialGenerator.WithConfiguration(materialConfig)
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
                                                       .WithMaterials(materials.NodesWithoutPurchase())
                                                       .Generate();

            // 1. Create Ressources with resourceConfig --> return actual capabilities and use them for next step

            MateDb mateDb = Dbms.GetMateDataBase(dbName: DataBaseConfiguration.MateDb).DbContext;
            mateDb.Database.EnsureDeleted();
            mateDb.Database.EnsureCreated();

            var masterTableCapabilities = CapbilityTransformer.Transform(mateDb, resourceConfig);

            // 2. Create Materials with materials.NodesInUse --> material

            MaterialTransformer.Transform(mateDb, materials, masterTableCapabilities);


            // 3. Create BOMS with materials.Edges

            // 4. Create Operations with materials.Operations

        }

    }
}
