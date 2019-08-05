using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor.Dsl;
using Master40.DB.Enums;
using Remotion.Linq.Parsing;

namespace Master40.Simulation.CLI.Arguments
{
    class SimType : ICommand
    {
        public string ArgLong => "SimType";
        public string ArgShort => "s";
        public bool HasProperty => true;
        public string Description => " -simtype <Type> : Specify simulation Type <Central/Decentral>";
        public Action<ParseResult, string> Action { get; }

        public SimType()
        {
            Action = (result, arg) =>
            {
                if (arg.Equals(SimulationType.Decentral.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    result.SimulationType = SimulationType.Decentral;
                }
                else if (arg.Equals(SimulationType.Central.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    result.SimulationType = SimulationType.Central;
                }
                else if (arg.Equals(SimulationType.Bucket.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    result.SimulationType = SimulationType.Bucket;
                }
                else
                {
                    throw  new Exception("Unknown argument.");
                }
                Console.WriteLine(result.SimulationType.ToString() + " has been selected!");
            };
        }
    }
}
