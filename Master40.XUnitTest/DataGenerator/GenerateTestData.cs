using System;
using System.Linq;
using Master40.DataGenerator.Configuration;
using Master40.DataGenerator.Generators;
using Master40.DataGenerator.DataModel.TransitionMatrix;
using Master40.DataGenerator.Util;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.Nominal.Model;
using Xunit;
using Xunit.Abstractions;
using InputParameterSet = Master40.DataGenerator.DataModel.ProductStructure.InputParameterSet;

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

            for (var i = 0; i < 1; i++)
            {
                //Nebenbedingung lautet, dass Fertigungstiefe mindestens 1 sein muss, es macht aber wenig Sinn, wenn sie gleich 1 ist, da es dann keine Fertigungen gibt
                //-> Anpassung der Nebenbedingung: Fertigungstiefe muss mindestens 2 sein
                //KG und MV nicht größer 5; FT nicht größer 20; Anzahl Endprodukte nicht größer 50
                InputParameterSet inputProductStructure;
                if (false) //wenn zufällige Eingabewerte generiert werden sollen
                {
                    inputProductStructure = new InputParameterSet
                    {
                        EndProductCount = rng.Next(9) + 2, DepthOfAssembly = rng.Next(10) + 1,
                        ComplexityRatio = rng.NextDouble() + 1, ReutilisationRatio = rng.NextDouble() + 1,
                        MeanIncomingMaterialAmount = 2.3, StdDevIncomingMaterialAmount = 0.7, MeanWorkPlanLength = 3,
                        VarianceWorkPlanLength = 1
                    };
                }
                else //wenn mit bestimmten Eingabewerten gearbeitet werden soll
                {
                    inputProductStructure = new InputParameterSet
                    {
                        EndProductCount = 7, DepthOfAssembly = 5, ComplexityRatio = 1.5, ReutilisationRatio = 1.8,
                        MeanIncomingMaterialAmount = 2.3, StdDevIncomingMaterialAmount = 0.7, MeanWorkPlanLength = 3,
                        VarianceWorkPlanLength = 1
                    };
                }

                _testOutputHelper.WriteLine(inputProductStructure.ToString());
                var productStructureGenerator = new ProductStructureGenerator();
                var productStructure = productStructureGenerator.GenerateProductStructure(inputProductStructure, articleTypes, units, unitCol);

                //Limit für Lambda und Anzahl Bearbeitungsstationen jeweils 100
                //Wenn MeanWorkPlanLength oder VarianceWorkPlanLength "null" sind, dann muss WithStartAndEnd "true" sein
                Master40.DataGenerator.DataModel.TransitionMatrix.InputParameterSet inputTransitionMatrix = null;
                var individualMachiningTime = true;

                inputTransitionMatrix = new Master40.DataGenerator.DataModel.TransitionMatrix.InputParameterSet
                {
                    DegreeOfOrganization = 0.7,
                    Lambda = 1.3,
                    InfiniteTools = true,
                    WithStartAndEnd = false,
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
                            WorkingStationCount = 5
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 10, VarianceMachiningTime = 4
                            },
                            CapabilitiesCount = 5,
                            WorkingStationCount = 2
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 13, VarianceMachiningTime = 7
                            },
                            CapabilitiesCount = 6,
                            WorkingStationCount = 5
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 20, VarianceMachiningTime = 3
                            },
                            CapabilitiesCount = 4,
                            WorkingStationCount = 4
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 3, VarianceMachiningTime = 1
                            },
                            CapabilitiesCount = 5,
                            WorkingStationCount = 4
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 12, VarianceMachiningTime = 2
                            },
                            CapabilitiesCount = 4,
                            WorkingStationCount = 3
                        },
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 10, VarianceMachiningTime = 7
                            },
                            CapabilitiesCount = 1,
                            WorkingStationCount = 1
                        }
                        
                    }
                };

                var transitionMatrixGenerator = new TransitionMatrixGenerator();
                var transitionMatrix = transitionMatrixGenerator.GenerateTransitionMatrix(inputTransitionMatrix, inputProductStructure);

                var resourceCapabilities = ResourceInitializer.MasterTableResourceCapability(dataBase.DbContext,
                    parameterSet.GetOption<Resource>().Value,
                    parameterSet.GetOption<Setup>().Value,
                    parameterSet.GetOption<Operator>().Value);

                var operationGenerator = new OperationGenerator();
                operationGenerator.GenerateOperations();

                var inputBillOfMaterial = new Master40.DataGenerator.DataModel.BillOfMaterial.InputParameterSet
                {
                    RoundEdgeWeight = false
                };

                var billOfMaterialGenerator = new BillOfMaterialGenerator();
                billOfMaterialGenerator.GenerateBillOfMaterial(inputBillOfMaterial, productStructure, transitionMatrix);

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

            Assert.True(true);
        }
    }
}
