using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.BusinessLogic.Simulation
{
    public class RandomNumbers { 
            public int RandomInt()
            {
                Random random = new Random();
                return random.Next(-1, 2);
            }
    }
}
