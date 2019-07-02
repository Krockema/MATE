using Master40.DB.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Helper
{
    public class CollectorAgentConfig
    {
        public string DBResultsConnectionString { get; }
        public int SimulationId { get; }
        public int SimulationNumber { get; }
        public SimulationType SimulationType { get; }
        public bool SaveToDB { get; }
        

        public CollectorAgentConfig(string dbResultConnectionString
                                    , bool saveToDB
                                    , int simulationId
                                    , int simulationNumber
                                    , SimulationType simulationType)
        {
            DBResultsConnectionString = dbResultConnectionString;
            SaveToDB = saveToDB;
            SimulationId = simulationId;
            SimulationNumber = simulationNumber;
            SimulationType = simulationType;
        }
    }
}
