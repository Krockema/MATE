using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.Configuration;
using Master40.DataGenerator.Generators;
using Master40.DataGenerator.DataModel.TransitionMatrix;
using Master40.DataGenerator.MasterTableInitializers;
using Master40.DataGenerator.Util;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.DynamicInitializer;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.DataModel;
using Master40.DB.Util;
using MathNet.Numerics.Distributions;
using Xunit;
using Xunit.Abstractions;
using InputParameterSet = Master40.DataGenerator.DataModel.ProductStructure.InputParameterSet;
using ResourceInitializer = Master40.DB.Data.DynamicInitializer.ResourceInitializer;

namespace Master40.XUnitTest.DataGenerator
{
    public class GenerateTestData
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public GenerateTestData(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        //Es gibt wohl eine Diskripanz zwischen Master40 und SYMTEP was Operationen und Stücklisten (BOM) angeht (Struktur und Zeitpunkt)
        [Fact]
        public void GenerateData()
        {
            var rng = new Random();

            var parameterSet = ParameterSet.Create(new object[] {Dbms.GetNewMasterDataBase(false, "Master40")});
            var dataBase = parameterSet.GetOption<DataBase<ProductionDomainContext>>();
            dataBase.DbContext.Database.EnsureDeleted();
            dataBase.DbContext.Database.EnsureCreated();
            var units = new MasterTableUnit();
            var unitCol = units.Init(dataBase.DbContext);
            var articleTypes = new MasterTableArticleType();
            articleTypes.Init(dataBase.DbContext);

            var iterations = 1;
            for (var i = 0; i < iterations; i++)
            {
                //Nebenbedingung lautet, dass Fertigungstiefe mindestens 1 sein muss, es macht aber wenig Sinn, wenn sie gleich 1 ist, da es dann keine Fertigungen gibt
                //-> Anpassung der Nebenbedingung: Fertigungstiefe muss mindestens 2 sein
                //KG und MV nicht größer 5; FT nicht größer 20; Anzahl Endprodukte nicht größer 50
                var randomGeneratedInputValues = false;
                var getWorkPlanLengthFromTransitionMatrix = true;
                var inputProductStructure = new InputParameterSet
                {
                    EndProductCount = !randomGeneratedInputValues ? 7 : rng.Next(9) + 2,
                    DepthOfAssembly = !randomGeneratedInputValues ? 5 : rng.Next(10) + 1,
                    ComplexityRatio = !randomGeneratedInputValues ? 1.5 : rng.NextDouble() + 1,
                    ReutilisationRatio = !randomGeneratedInputValues ? 1.8 : rng.NextDouble() + 1,
                    MeanIncomingMaterialAmount = 2.3, StdDevIncomingMaterialAmount = 0.7,
                    MeanWorkPlanLength = getWorkPlanLengthFromTransitionMatrix ? double.NaN : 3,
                    VarianceWorkPlanLength = getWorkPlanLengthFromTransitionMatrix ? double.NaN : 1
                };
                _testOutputHelper.WriteLine(inputProductStructure.ToString());
                var productStructureGenerator = new ProductStructureGenerator();
                var productStructure = productStructureGenerator.GenerateProductStructure(inputProductStructure, articleTypes, units, unitCol);
                ArticleInitializer.Init(productStructure.NodesPerLevel, dataBase.DbContext);

                var articleTable = dataBase.DbContext.Articles.ToArray();
                MasterTableStock.Init(dataBase.DbContext, articleTable);

                //Limit für Lambda und Anzahl Bearbeitungsstationen jeweils 100
                //Wenn MeanWorkPlanLength oder VarianceWorkPlanLength "null" sind, dann muss WithStartAndEnd "true" sein
                var individualMachiningTime = true;
                var inputTransitionMatrix = new Master40.DataGenerator.DataModel.TransitionMatrix.InputParameterSet
                {
                    DegreeOfOrganization = 0.7,
                    Lambda = 1.3,
                    InfiniteTools = true,
                    WithStartAndEnd = getWorkPlanLengthFromTransitionMatrix,
                    GeneralMachiningTimeParameterSet = individualMachiningTime ? null : new MachiningTimeParameterSet
                    {
                        MeanMachiningTime = 15,
                        VarianceMachiningTime = 5
                    },
                    WorkingStations = new WorkingStationParameterSet[]
                    {
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 15, VarianceMachiningTime = 5
                            },
                            CapabilitiesCount = 3,
                            ToolCount = 5,
                            SetupTime = 10,
                            OperatorCount = 0
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 10, VarianceMachiningTime = 4
                            },
                            CapabilitiesCount = 5,
                            ToolCount = 2,
                            SetupTime = 3,
                            OperatorCount = 0
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 13, VarianceMachiningTime = 7
                            },
                            CapabilitiesCount = 6,
                            ToolCount = 5,
                            SetupTime = 0,
                            OperatorCount = 1
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 20, VarianceMachiningTime = 3
                            },
                            CapabilitiesCount = 4,
                            ToolCount = 4,
                            SetupTime = 12,
                            OperatorCount = 1
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 3, VarianceMachiningTime = 1
                            },
                            CapabilitiesCount = 5,
                            ToolCount = 4,
                            SetupTime = 10,
                            OperatorCount = 1
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 12, VarianceMachiningTime = 2
                            },
                            CapabilitiesCount = 4,
                            ToolCount = 3,
                            SetupTime = 1,
                            OperatorCount = 0
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 10, VarianceMachiningTime = 7
                            },
                            CapabilitiesCount = 1,
                            ToolCount = 1,
                            SetupTime = 5,
                            OperatorCount = 0
                        }
                        
                    }
                };

                var transitionMatrixGenerator = new TransitionMatrixGenerator();
                var transitionMatrix = transitionMatrixGenerator.GenerateTransitionMatrix(inputTransitionMatrix, inputProductStructure);

                List<ResourceProperty> resourceProperties =
                    inputTransitionMatrix.WorkingStations.Select(x => (ResourceProperty) x).ToList();

                var resourceCapabilities = ResourceInitializer.Initialize(dataBase.DbContext, resourceProperties);

                var operationGenerator = new OperationGenerator();
                operationGenerator.GenerateOperations(productStructure.NodesPerLevel, transitionMatrix,
                    inputTransitionMatrix, resourceCapabilities);
                OperationInitializer.Init(productStructure.NodesPerLevel, dataBase.DbContext);

                var inputBillOfMaterial = new Master40.DataGenerator.DataModel.BillOfMaterial.InputParameterSet
                {
                    RoundEdgeWeight = false,
                    WeightEpsilon = 0.001m
                };

                var billOfMaterialGenerator = new BillOfMaterialGenerator();
                billOfMaterialGenerator.GenerateBillOfMaterial(inputBillOfMaterial, productStructure.NodesPerLevel, transitionMatrix, units);
                BillOfMaterialInitializer.Init(productStructure.NodesPerLevel, dataBase.DbContext);

                var businessPartner = new MasterTableBusinessPartner();
                businessPartner.Init(dataBase.DbContext);

                dataBase.DbContext.SaveChanges();

                List<M_ArticleToBusinessPartner> articleToBusinessPartners = new List<M_ArticleToBusinessPartner>();
                foreach (var article in articleTable)
                {
                    articleToBusinessPartners.Add(new M_ArticleToBusinessPartner()
                    {
                        BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                        ArticleId = article.Id,
                        PackSize = 10,
                        Price = 20.00,
                        TimeToDelivery = 480
                    });
                }

                dataBase.DbContext.ArticleToBusinessPartners.AddRange(articleToBusinessPartners);
                dataBase.DbContext.SaveChanges();

                Assert.True(productStructure.NodesPerLevel.Count == inputProductStructure.DepthOfAssembly &&
                            productStructure.NodesPerLevel[0].Count == inputProductStructure.EndProductCount);
            }
        }

        //maximale Anzahl an Bearbeitungsstationen: 21
        [Fact]
        public void Test1()
        {
            var lintMax = Int32.MaxValue;
            var longMax = Int64.MaxValue;
            var doubleMax = Double.MaxValue;
            _testOutputHelper.WriteLine(lintMax.ToString());
            _testOutputHelper.WriteLine(longMax.ToString());
            _testOutputHelper.WriteLine(doubleMax.ToString());
            var faculty = new Faculty();
            //var f1 = faculty.Calc(200);
            //var f2 = faculty.Calc(20);
            //var r = Math.Round(f2);
            //var p1 = Math.Pow(100.537, 100);
            //var p2 = Math.Pow(10.1, 20);
            var x1 = Math.Round(Math.Pow(5.0 / 1.0, 19) * 50);
            var x2 = Convert.ToInt64(x1);
            var sum1 = Convert.ToInt64(0);
            var sum2 = 0.0;
            for (int i = 0; i < 20; i++)
            {
                var result = Convert.ToInt64(Math.Round(Math.Pow(5.0 / 1.0, i) * 50));
                sum1 += result;
                sum2 += result;
            }

            sum1 *= 5;
            sum2 *= 5.0;
            var x3 = Convert.ToInt64(Math.Round(sum2));

            var x4 = Math.Round(5.4343454359);

            var n1 = AlphabeticNumbering.GetAlphabeticNumbering(0);
            var n2 = AlphabeticNumbering.GetAlphabeticNumbering(25);
            var n3 = AlphabeticNumbering.GetAlphabeticNumbering(26);
            var n4 = AlphabeticNumbering.GetAlphabeticNumbering(52);
            var n5 = AlphabeticNumbering.GetAlphabeticNumbering(454);
            var n6 = AlphabeticNumbering.GetAlphabeticNumbering(1);
            var n7 = AlphabeticNumbering.GetAlphabeticNumbering(2);
            var n8 = AlphabeticNumbering.GetAlphabeticNumbering(3);

            var list1 = new List<TruncatedDiscreteNormal>();
            var truncatedDiscreteNormalDistribution =
                new TruncatedDiscreteNormal(9, 11, Normal.WithMeanVariance(5.0, 2.0));
            list1.Add(truncatedDiscreteNormalDistribution);
            list1.Add(truncatedDiscreteNormalDistribution);
            list1.Add(truncatedDiscreteNormalDistribution);
            list1.Add(truncatedDiscreteNormalDistribution);
            var x5 = list1[1].Sample();

            Assert.True(true);
        }
    }
}
