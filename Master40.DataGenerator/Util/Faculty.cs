using System.Collections.Generic;

namespace Master40.DataGenerator.Util
{
    public class Faculty
    {

        private readonly List<double> _facultyResults;

        public Faculty()
        {
            _facultyResults = new List<double>();
        }

        public double Calc(int value)
        {
            if (_facultyResults.Count <= value)
            {
                for (int i = _facultyResults.Count; i <= value; i++)
                {
                    if (i == 0)
                    {
                        _facultyResults.Add(1);
                    }
                    else
                    {
                        _facultyResults.Add(_facultyResults[i - 1] * i);
                    }
                }
            }
            return _facultyResults[value];
        }

    }
}