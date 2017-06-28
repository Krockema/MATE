using System;

namespace Master40.Simulation.Simulation
{
    public class RandomNumbers { 
            public int RandomInt()
            {
                Random random = new Random();
                return random.Next(-1, 2);
            }
    }
}
