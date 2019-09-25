﻿using Master40.DB.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.ReportingModel.Interface
{
    public interface ISimulationResourceData
    {
        int SimulationConfigurationId { get; set; }
        SimulationType SimulationType { get; set; }
        int SimulationNumber { get; set; }
        long Time { get; set; }
        long ExpectedDuration { get; set; }
        long Start { get; set; }
        long End { get; set; }
        string Resource { get; set; }
        string ResourceTool { get; set; }
    }
}
