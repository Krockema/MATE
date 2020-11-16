using System;
using Master40.DataGenerator.DataModel;
using Master40.DataGenerator.Util;
using Master40.DB.GeneratorModel;

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

        public TransitionMatrix GenerateTransitionMatrix(TransitionMatrixInput inputParameters,
            ProductStructureInput inputProductStructure, XRandom rng)
        {
            var jCorrector = 0;
            var matrixSize = inputParameters.WorkingStations.Count;
            if (inputParameters.ExtendedTransitionMatrix)
            {
                matrixSize += 1;
                jCorrector = 1;
            }
            _piA = new double[matrixSize, matrixSize];
            _piB = new double[matrixSize, matrixSize];

            InitializePiA(inputParameters, rng, matrixSize, jCorrector);
            InitializePiB(inputParameters, inputProductStructure, matrixSize, jCorrector);
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

        private void InitializePiA(TransitionMatrixInput inputParameters, XRandom rng, int matrixSize, int jCorrector)
        {
            var faculty = new Faculty();
            for (var i = 0; i < matrixSize; i++)
            {
                var lineSum = 0.0;
                for (var j = 0; j < matrixSize; j++)
                {
                    if (i != j + jCorrector)
                    {
                        var cellValue =
                            InitializePiACalcCellValue(inputParameters, i, j, matrixSize, faculty, rng, jCorrector);
                        _piA[i, j] = cellValue;
                        lineSum += cellValue;
                    }
                }

                for (var j = 0; j < matrixSize; j++)
                {
                    _piA[i, j] = _piA[i, j] / lineSum;
                }
            }

            _organizationDegreeA = CalcOrganizationDegree(_piA, matrixSize);
        }

        private double InitializePiACalcCellValue(TransitionMatrixInput inputParameters, int i, int j, int matrixSize, Faculty faculty, XRandom rng, int jCorrector)
        {
            var noiseFactor = 1 + 0.2 * (rng.NextDouble() - 0.5);
            if (inputParameters.ExtendedTransitionMatrix && i == 0 && j + 1 == matrixSize)
            {
                return 0.0;
            }
            if (i < j + jCorrector)
            {
                return Math.Pow(inputParameters.Lambda, -i + j + jCorrector - 1) / faculty.Calc(-i + j + jCorrector - 1) * noiseFactor;
            }
            return Math.Pow(inputParameters.Lambda, i - (j + jCorrector) - 1) / (2 * faculty.Calc(i - (j + jCorrector) - 1)) * noiseFactor;
        }

        private void InitializePiB(TransitionMatrixInput inputParameters, ProductStructureInput inputProductStructure, int matrixSize, int jCorrector)
        {
            if (_organizationDegreeA > inputParameters.DegreeOfOrganization)
            {
                for (var i = 0; i < matrixSize; i++)
                {
                    for (var j = 0; j < matrixSize; j++)
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
                for (var i = 0; i < matrixSize; i++)
                {
                    for (var j = 0; j < matrixSize; j++)
                    {
                        /*
                        if (i < matrixSize - 1 && j + jCorrector == i + 1)
                        {
                            _piB[i, j] = 1.0;
                        }
                        else if (i == matrixSize - 1 && j + jCorrector == jForSpecialCase - 1)
                        {
                            _piB[i, j] = 1.0;
                        }
                        */
                        // Es wurde entschieden, dass in diesem Fall die Matrix piB wie eine ("verschobene") Einheitsmatrix initiiert werden soll, damit jede Maschine eine andere Folgemaschine besitzt.
                        // Grund: Wegen dem speziellen Fall, der für i = M-1 gilt, wo eine 1 gesetzt wird bei j = trunc(M - M/FT + 1) und dem dadurch verursachden Problem, dass (fast) nie zur ersten Maschine zurückgekerht werden kann (bei hohen OGs) und zusätzlich eine Übergangsschleife zwischen einigen Maschinen verursacht wird.
                        if ((j + jCorrector) % matrixSize == (i + 1) % matrixSize)
                        {
                            _piB[i, j] = 1.0;
                        }
                    }
                }

                _organizationDegreeB = 1.0;
            }
        }

        private void Bisection(TransitionMatrixInput inputParameters, int matrixSize)
        {
            
            _piC = new double[matrixSize, matrixSize];
            for (var i = 0; i < matrixSize; i++)
            {
                var lineSum = 0.0;
                for (var j = 0; j < matrixSize; j++)
                {
                    var cellValue = 0.5 * (_piA[i, j] + _piB[i, j]);
                    if (inputParameters.ExtendedTransitionMatrix && i == 0 && j + 1 == matrixSize)
                    {
                        cellValue = 0.0;
                    }
                    _piC[i, j] = cellValue;
                    if (i == 0 && inputParameters.ExtendedTransitionMatrix)
                    {
                        lineSum += cellValue;
                    }
                }

                if (i == 0 && inputParameters.ExtendedTransitionMatrix)
                {
                    for (var j = 0; j < matrixSize; j++)
                    {
                        _piC[i, j] = _piC[i, j] / lineSum;
                    }
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

        public double CalcOrganizationDegree(double[,] pi, int matrixSize)
        {
            var sum = 0.0;
            var reciprocalOfPiSize = 1.0 / matrixSize;
            foreach (var cell in pi)
            {
                sum += Math.Pow(cell - reciprocalOfPiSize, 2);
            }
            return 1.0 / (matrixSize - 1) * sum;
        }

        public void OutputMatrixAsCsv(double[,] pi, int matrixSize)
        {
            var output = "\n";
            for (var i = 0; i < matrixSize; i++)
            {
                for (var j = 0; j < matrixSize; j++)
                {
                    if (j != 0)
                    {
                        output += ",";
                    }
                    output += "\"" + Math.Round(pi[i, j], 3) + "\"";
                }
                output += "\n";
            }
            System.Diagnostics.Debug.WriteLine(output);
        }

    }
}
