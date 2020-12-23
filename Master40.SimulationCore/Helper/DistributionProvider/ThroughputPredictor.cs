using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.ReportingModel;

namespace Master40.SimulationCore.Helper.DistributionProvider
{
    public class ThroughputPredictor
    {
        public ThroughputPredictor()
        {
        }
        public long PredictThroughput(IEnumerable<Kpi> valuesForPrediction)
        {
            //_estimatedThroughPuts.UpdateOrCreate(articleName, predictedThroughput);
            return 1920;
        }
    }
}
