using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.DataModel;

namespace Master40.SimulationCore.Helper
{
    public static class Calculations
    {
        public static int GetTransitionTimeForWorkSchedule(T_ProductionOrderOperation schedule)
        {
            var transitionTime = 0;
            switch (schedule.MachineGroupId)
            {
                case 1: transitionTime = 90; break;
                case 2: transitionTime = 210; break;
                case 3: transitionTime = 100; break;
                default: transitionTime = 0; break;
            }

            return transitionTime;
        }

    }
}
