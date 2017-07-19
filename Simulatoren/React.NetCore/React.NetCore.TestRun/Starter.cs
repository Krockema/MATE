using System;
using React;

namespace React_Beispielapp
{
    public class Starter
    { 
        public static void Main()
        {
            Factory fac = new Factory();
            Task generator = new Process(fac, fac.Generator);
            fac.Run(generator);
            Console.ReadLine();

        }
    }
}