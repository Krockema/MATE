using System;
using System.Collections.Generic;
using Master40.DataGenerator.DataModel.TransitionMatrix;
using Master40.DataGenerator.Util;

namespace Master40.DataGenerator.Generators
{
    public class TransitionMatrixGenerator
    {

        private double[,] _piA;

        private double _organizationDegreeA;

        private double[,] _piB;

        private double _organizationDegreeB;

        private double[,] _piC;

        private double _organizationDegreeC;

        public TransitionMatrix GenerateTransitionMatrix(InputParameterSet inputParameters, DataModel.ProductStructure.InputParameterSet inputProductStructure)
        {
            var matrixSize = inputParameters.WorkingStationCount;
            if (inputParameters.WithStartAndEnd)
            {
                matrixSize += 2;
            }
            _piA = new double[matrixSize, matrixSize];
            _piB = new double[matrixSize, matrixSize];

            var rng = new XRandom();

            InitializePiA(inputParameters, rng, matrixSize);
            InitializePiB(inputParameters, inputProductStructure, matrixSize);
            while (Math.Abs(_organizationDegreeA - inputParameters.DegreeOfOrganization) > 0.001)
            {
                Bisection(inputParameters, matrixSize);
            }
            var transitionMatrix = new TransitionMatrix
            {
                Pi = _piA
            };
            return transitionMatrix;
        }

        private void InitializePiA(InputParameterSet inputParameters, XRandom rng, int matrixSize)
        {
            var faculty = new Faculty();
            for (int i = 0; i < matrixSize; i++)
            {
                var lineSum = 0.0;
                for (int j = 0; j < matrixSize; j++)
                {
                    if (i != j)
                    {
                        var cellValue =
                            InitializePiACalcCellValue(inputParameters, i, j, i < j, matrixSize, faculty, rng);
                        _piA[i, j] = cellValue;
                        lineSum += cellValue;
                    }
                    else if (inputParameters.WithStartAndEnd && i + 1 == matrixSize && i == j)
                    {
                        _piA[i, j] = 1.0;
                        lineSum += 1;
                    }
                }

                for (int j = 0; j < matrixSize; j++)
                {
                    _piA[i, j] = _piA[i, j] / lineSum;
                }
            }

            _organizationDegreeA = CalcOrganizationDegree(_piA, matrixSize);
        }

        private double InitializePiACalcCellValue(InputParameterSet inputParameters, int i, int j, bool iLessThanJ, int matrixSize, Faculty faculty, XRandom rng)
        {
            var noiseFactor = 1 + 0.2 * (rng.NextDouble() - 0.5);
            if (inputParameters.WithStartAndEnd && (j == 0 || i + 1 == matrixSize || (i == 0 && j + 1 == matrixSize)))
            {
                return 0.0;
            }
            if (iLessThanJ)
            {
                return Math.Pow(inputParameters.Lambda, -i + j - 1) / faculty.Calc(-i + j - 1) * noiseFactor;
            }
            return Math.Pow(inputParameters.Lambda, i - j - 1) / (2 * faculty.Calc(i - j - 1)) * noiseFactor;
        }

        private void InitializePiB(InputParameterSet inputParameters, DataModel.ProductStructure.InputParameterSet inputProductStructure, int matrixSize)
        {
            if (_organizationDegreeA > inputParameters.DegreeOfOrganization)
            {
                for (int i = 0; i < matrixSize; i++)
                {
                    for (int j = 0; j < matrixSize; j++)
                    {
                        _piB[i, j] = 1.0 / matrixSize;
                    }
                }

                _organizationDegreeB = 0.0;
            }

            else
            {
                var jForSpecialCase = Convert.ToInt32(Math.Truncate(
                    matrixSize - matrixSize /
                    Convert.ToDecimal(inputProductStructure.DepthOfAssembly) + 1));
                for (int i = 0; i < matrixSize; i++)
                {
                    for (int j = 0; j < matrixSize; j++)
                    {
                        if (i < matrixSize - 1 && j == i + 1)
                        {
                            _piB[i, j] = 1.0;
                        }
                        else if (i == matrixSize - 1 && j == jForSpecialCase - 1)
                        {
                            _piB[i, j] = 1.0;
                        }
                    }
                }

                _organizationDegreeB = 1.0;
            }
        }

        private void Bisection(InputParameterSet inputParameters, int matrixSize)
        {
            
            _piC = new double[matrixSize, matrixSize];
            for (int i = 0; i < matrixSize; i++)
            {
                var lineSum = 0.0;
                for (int j = 0; j < matrixSize; j++)
                {
                    var cellValue = 0.5 * (_piA[i, j] + _piB[i, j]);
                    if (inputParameters.WithStartAndEnd)
                    {
                        if (i + 1 == matrixSize && i == j)
                        {
                            cellValue = 1.0;
                        }
                        else if ((j == 0 || i + 1 == matrixSize || (i == 0 && j + 1 == matrixSize)))
                        {
                            cellValue = 0.0;
                        }
                    }
                    _piC[i, j] = cellValue;
                    lineSum += cellValue;
                }
                for (int j = 0; j < matrixSize; j++)
                {
                    _piC[i, j] = _piC[i, j] / lineSum;
                }
            }

            _organizationDegreeC = CalcOrganizationDegree(_piC, matrixSize);

            var signA = (_organizationDegreeA - inputParameters.DegreeOfOrganization) < 0;
            var signC = (_organizationDegreeC - inputParameters.DegreeOfOrganization) < 0;
            if (signA == signC)
            {
                _piA = _piC;
                _organizationDegreeA = _organizationDegreeC;
            }
            else
            {
                _piB = _piC;
                _organizationDegreeB = _organizationDegreeC;
            }
        }

        private double CalcOrganizationDegree(double[,] pi, int matrixSize)
        {
            var sum = 0.0;
            var reciprocalOfPiSize = 1.0 / matrixSize;
            foreach (var cell in pi)
            {
                sum += Math.Pow(cell - reciprocalOfPiSize, 2);
            }
            return 1.0 / (matrixSize - 1) * sum;
        }

    }
}
