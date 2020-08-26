using System;
using Master40.DataGenerator.Generators;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DataGenerator.Util;
using Xunit;
using Xunit.Abstractions;

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
                        MeanIncomingMaterialAmount = 2.3, VarianceIncomingMaterialAmount = 0.7
                    };
                }
                else //wenn mit bestimmten Eingabewerten gearbeitet werden soll
                {
                    inputProductStructure = new InputParameterSet
                    {
                        EndProductCount = 7, DepthOfAssembly = 5, ComplexityRatio = 1.5, ReutilisationRatio = 1.8,
                        MeanIncomingMaterialAmount = 2.3, VarianceIncomingMaterialAmount = 0.7
                    };
                }

                _testOutputHelper.WriteLine(inputProductStructure.ToString());
                var productStructureGenerator = new ProductStructureGenerator();
                var productStructure = productStructureGenerator.GenerateProductStructure(inputProductStructure);

                //Limit für Lambda und Anzahl Bearbeitungsstationen jeweils 100
                var inputTransitionMatrix = new Master40.DataGenerator.DataModel.TransitionMatrix.InputParameterSet
                {
                    DegreeOfOrganization = 1.0, Lambda = 1.3, WorkingStationCount = 7, WithStartAndEnd = true
                };
                var transitionMatrixGenerator = new TransitionMatrixGenerator();
                var transitionMatrix = transitionMatrixGenerator.GenerateTransitionMatrix(inputTransitionMatrix, inputProductStructure);

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
            Assert.True(true);
        }
    }
}
