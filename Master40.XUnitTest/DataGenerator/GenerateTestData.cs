﻿using Master40.DataGenerator.Generators;
using Master40.DataGenerator.Repository;
using Master40.DataGenerator.Util;
using Master40.DataGenerator.Verification;
using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.GeneratorModel;
using Master40.DB.Util;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Master40.XUnitTest.DataGenerator
{
    public class GenerateTestData
    {
        
        //Es gibt wohl eine Diskripanz zwischen Master40 und SYMTEP was Operationen und Stücklisten (BOM) angeht (Struktur und Zeitpunkt)
        [Fact]
        public void SetInput()
        {
            var success = true;
            var iterations = 0.9;

            for (var i = 0.1; i <= iterations; i += .1)
            {
                var usePresetSeed = true;
                var rng = new Random();
                int seed = usePresetSeed ? 368200759 : rng.Next();

                var generatorDbCtx = Dbms.GetGeneratorDataBase("TestGeneratorContext").DbContext;
                var approach = new Approach()
                {
                    CreationDate = DateTime.Now,
                    Seed = seed
                };

                //Limit für Lambda und Anzahl Bearbeitungsstationen jeweils 100
                var individualMachiningTime = true;
                approach.TransitionMatrixInput = new TransitionMatrixInput
                {
                    DegreeOfOrganization = i,
                    Lambda = 1.3,
                    InfiniteTools = true,
                    ExtendedTransitionMatrix = false,
                    GeneralMachiningTimeParameterSet = individualMachiningTime ? null : new MachiningTimeParameterSet
                    {
                        MeanMachiningTime = 1,
                        VarianceMachiningTime = 0
                    },
                    WorkingStations = new List<WorkingStationParameterSet>()
                };

                for (int j = 0; j < 4; j++)
                {
                    approach.TransitionMatrixInput.WorkingStations.Add(
                        new WorkingStationParameterSet()
                        {
                            MachiningTimeParameterSet = !individualMachiningTime ? null : new MachiningTimeParameterSet
                            {
                                MeanMachiningTime = 10, VarianceMachiningTime = 2
                            },
                            ResourceCount = 4,
                            ToolCount = 6,
                            SetupTime = 30,
                            OperatorCount = 1
                        });
                }



                //Nebenbedingung lautet, dass Fertigungstiefe mindestens 1 sein muss, es macht aber wenig Sinn, wenn sie gleich 1 ist, da es dann keine Fertigungen gibt
                //-> Anpassung der Nebenbedingung: Fertigungstiefe muss mindestens 2 sein
                //KG und MV nicht größer 5; FT nicht größer 20; Anzahl Endprodukte nicht größer 50
                var randomGeneratedInputValues = false;
                double? doubleNull = null;
                approach.ProductStructureInput = new ProductStructureInput
                {
                    EndProductCount = !randomGeneratedInputValues ? 35 : rng.Next(9) + 2, // 100
                    DepthOfAssembly = !randomGeneratedInputValues ? 4 : rng.Next(10) + 1, 
                    ComplexityRatio = !randomGeneratedInputValues ? 1.41: rng.NextDouble() + 1, // 2 
                    ReutilisationRatio = !randomGeneratedInputValues ? 1 : rng.NextDouble() + 1, // 5
                    MeanIncomingMaterialAmount = 1,
                    StdDevIncomingMaterialAmount = 0,
                    MeanWorkPlanLength = approach.TransitionMatrixInput.ExtendedTransitionMatrix ? doubleNull : 5.0,
                    VarianceWorkPlanLength = approach.TransitionMatrixInput.ExtendedTransitionMatrix ? doubleNull : 0.0
                };
                //System.Diagnostics.Debug.WriteLine(approach.ProductStructureInput.ToString());

                approach.BomInput = new BillOfMaterialInput
                {
                    RoundEdgeWeight = true,
                    WeightEpsilon = 0.001m
                };

                generatorDbCtx.Approaches.AddRange(approach);
                generatorDbCtx.SaveChanges();

                System.Diagnostics.Debug.WriteLine("################################# Generated test data have the approach id of " + approach.Id);
            }

            Assert.True(success);
        }

        [Fact]
        public void GenerateData() //Generierung für Simulation direkt im Testfall, wo Simulation durchgeführt wird
        {
            var approachRangeStart = 17;
            var approachRangeEnd = 17;
            for (var i = approachRangeStart; i < approachRangeEnd + 1; i++)
            {
                var approachId = i;
                var generatorDbCtx = Dbms.GetGeneratorDataBase("TestGeneratorContext").DbContext;
                var approach = ApproachRepository.GetApproachById(generatorDbCtx, approachId);

                /*var parameterSet = ParameterSet.Create(new object[] { Dbms.GetNewMasterDataBase(false, "Master40") });
                var dataBase = parameterSet.GetOption<DataBase<ProductionDomainContext>>();*/

                var dbContext = Dbms.GetMasterDataBase(dbName: "Test").DbContext;
                var resultContext = Dbms.GetResultDataBase("TestResults").DbContext;

                var generator = new MainGenerator();
                generator.StartGeneration(approach, dbContext, resultContext, true, 0.4);
            }
            Assert.True(true);

        }

        [Fact]
        public void CheckOrganizationDegreeFromResults()
        {
            var simNumber = 27;
            var dbContext = Dbms.GetMasterDataBase(dbName: "Test").DbContext;
            var dbResultCtx = Dbms.GetResultDataBase("TestResults").DbContext;
            var dbGeneratorCtx = Dbms.GetGeneratorDataBase("TestGeneratorContext").DbContext;

            var transitionMatrixGeneratorVerifier = new TransitionMatrixGeneratorVerifier();
            transitionMatrixGeneratorVerifier.VerifySimulatedData(dbContext, dbGeneratorCtx, dbResultCtx, simNumber);

            Assert.True(true);
        }

        //maximale Anzahl an Bearbeitungsstationen: 21
        [Fact]
        public void Test1()
        {
            var lintMax = Int32.MaxValue;
            var longMax = Int64.MaxValue;
            var doubleMax = Double.MaxValue;
            System.Diagnostics.Debug.WriteLine(lintMax.ToString());
            System.Diagnostics.Debug.WriteLine(longMax.ToString());
            System.Diagnostics.Debug.WriteLine(doubleMax.ToString());
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

            var n11 = AlphabeticNumbering.GetNumericRepresentation(n1);
            var n12 = AlphabeticNumbering.GetNumericRepresentation(n2);
            var n13 = AlphabeticNumbering.GetNumericRepresentation(n3);
            var n14 = AlphabeticNumbering.GetNumericRepresentation(n4);
            var n15 = AlphabeticNumbering.GetNumericRepresentation(n5);
            var n16 = AlphabeticNumbering.GetNumericRepresentation(n6);
            var n17 = AlphabeticNumbering.GetNumericRepresentation(n7);
            var n18 = AlphabeticNumbering.GetNumericRepresentation(n8);

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
