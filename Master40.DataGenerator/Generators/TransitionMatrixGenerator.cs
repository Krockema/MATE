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
            _piA = new double[inputParameters.WorkingStationCount, inputParameters.WorkingStationCount];
            _piB = new double[inputParameters.WorkingStationCount, inputParameters.WorkingStationCount];

            var rng = new XRandom();

            InitializePiA(inputParameters, rng);
            InitializePiB(inputParameters, inputProductStructure);
            while (Math.Abs(_organizationDegreeA - inputParameters.DegreeOfOrganization) > 0.001)
            {
                Bisection(inputParameters);
            }
            var transitionMatrix = new TransitionMatrix
            {
                Pi = _piA
            };
            return transitionMatrix;
        }

        private void InitializePiA(InputParameterSet inputParameters, XRandom rng)
        {
            var organizationDegreeHelper = 0.0;
            var faculty = new Faculty();
            for (int i = 0; i < inputParameters.WorkingStationCount; i++)
            {
                var lineSum = 0.0;
                for (int j = 0; j < inputParameters.WorkingStationCount; j++)
                {
                    var noiceFactor = 1 + 0.2 * (rng.NextDouble() - 0.5);
                    if (i < j)
                    {
                        _piA[i, j] = Math.Pow(inputParameters.Lambda, -i + j - 1) / faculty.Calc(-i + j - 1) *
                                     noiceFactor;
                        lineSum += _piA[i, j];
                    }
                    else if (i > j)
                    {
                        _piA[i, j] = Math.Pow(inputParameters.Lambda, i - j - 1) / (2 * faculty.Calc(i - j - 1)) *
                                     noiceFactor;
                        lineSum += _piA[i, j];
                    }
                }

                for (int j = 0; j < inputParameters.WorkingStationCount; j++)
                {
                    _piA[i, j] = _piA[i, j] / lineSum;
                    organizationDegreeHelper += Math.Pow(_piA[i, j] - 1.0 / inputParameters.WorkingStationCount, 2);
                }
            }

            _organizationDegreeA =
                1.0 / (inputParameters.WorkingStationCount - 1) * organizationDegreeHelper;
        }

        private void InitializePiB(InputParameterSet inputParameters, DataModel.ProductStructure.InputParameterSet inputProductStructure)
        {
            if (_organizationDegreeA > inputParameters.DegreeOfOrganization)
            {
                for (int i = 0; i < inputParameters.WorkingStationCount; i++)
                {
                    for (int j = 0; j < inputParameters.WorkingStationCount; j++)
                    {
                        _piB[i, j] = 1.0 / inputParameters.WorkingStationCount;
                    }
                }

                _organizationDegreeB = 0.0;
            }

            else
            {
                var jForSpecialCase = Convert.ToInt32(Math.Truncate(
                    inputParameters.WorkingStationCount - inputParameters.WorkingStationCount /
                    Convert.ToDecimal(inputProductStructure.DepthOfAssembly) + 1));
                for (int i = 0; i < inputParameters.WorkingStationCount; i++)
                {
                    for (int j = 0; j < inputParameters.WorkingStationCount; j++)
                    {
                        if (i < inputParameters.WorkingStationCount - 1 && j == i + 1)
                        {
                            _piB[i, j] = 1.0;
                        }
                        else if (i == inputParameters.WorkingStationCount - 1 && j == jForSpecialCase - 1)
                        {
                            _piB[i, j] = 1.0;
                        }
                    }
                }

                _organizationDegreeB = 1.0;
            }
        }

        private void Bisection(InputParameterSet inputParameters)
        {
            var organizationDegreeHelper = 0.0;
            _piC = new double[inputParameters.WorkingStationCount, inputParameters.WorkingStationCount];
            for (int i = 0; i < inputParameters.WorkingStationCount; i++)
            {
                for (int j = 0; j < inputParameters.WorkingStationCount; j++)
                {
                    _piC[i, j] = 0.5 * (_piA[i, j] + _piB[i, j]);
                    organizationDegreeHelper += Math.Pow(_piC[i, j] - 1.0 / inputParameters.WorkingStationCount, 2);
                }
            }

            _organizationDegreeC = 1.0 / (inputParameters.WorkingStationCount - 1) * organizationDegreeHelper;

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

    }
}
